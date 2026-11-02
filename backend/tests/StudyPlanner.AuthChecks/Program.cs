using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using NpgsqlTypes;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

var supplied = Environment.GetEnvironmentVariable("STUDYPLANNER_TEST_POSTGRES");
if (string.IsNullOrWhiteSpace(supplied)) throw new InvalidOperationException("Use isolated local test PostgreSQL.");
var config = new NpgsqlConnectionStringBuilder(supplied);
if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres") throw new InvalidOperationException("Local test DB only.");
var database = "studyplanner_auth_checks_" + Guid.NewGuid().ToString("N");
await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
config.Database = database;
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
WebApplication? app = null;
var checks = 0;
void Check(bool condition, string label) { if (!condition) throw new Exception("FAIL: " + label); checks++; }
async Task VerifyColumnRenameMigrationAsync()
{
    // Fixture độc lập với model hiện tại: ghi schema cũ bằng SQL tham số hóa, không đọc file obj khi chạy.
    const string oldMigration = "20261014160645_CreateStudyPlannerSchema";
    const string renameMigration = "20261031070326_RenameBusinessFieldsToVietnamese";
    const string profileMigration = "20261031073854_DecoupleStudentProfileFromCatalogs";
    const string tableMigration = "20261031093353_RenameBusinessTablesToVietnamese";
    const string columnPairs = """
        Id=ma Code=ky_hieu Name=ten SchoolId=ma_truong FacultyId=ma_khoa CohortId=ma_khoa_hoc ClassId=ma_lop
        UserId=ma_nguoi_dung SemesterId=ma_hoc_ky CourseId=ma_mon_hoc TaskId=ma_nhiem_vu ParentTaskId=ma_nhiem_vu_cha
        Email=email NormalizedEmail=email_chuan_hoa FullName=ho_ten PasswordHash=mat_khau_bam StudentCode=ma_so_sinh_vien
        Major=nganh_hoc Role=vai_tro IsActive=dang_hoat_dong TimeZone=mui_gio ReminderHours=nhac_truoc_gio
        CreatedAt=ngay_tao UpdatedAt=ngay_cap_nhat StartDate=ngay_bat_dau EndDate=ngay_ket_thuc
        StartYear=nam_bat_dau EndYear=nam_ket_thuc Credits=so_tin_chi Lecturer=giang_vien Color=mau_sac
        DayOfWeek=thu_trong_tuan StartTime=gio_bat_dau EndTime=gio_ket_thuc Room=phong_hoc
        Title=tieu_de Description=mo_ta Type=loai DueDate=han_hoan_thanh Priority=muc_do_uu_tien Status=trang_thai
        EstimatedMinutes=thoi_luong_du_kien_phut Progress=tien_do CompletedAt=ngay_hoan_thanh
        Content=noi_dung RemindAt=thoi_diem_nhac DeliveryStatus=trang_thai_phat SentAt=ngay_phat ReadAt=ngay_doc
        AttemptCount=so_lan_thu NextAttemptAt=lan_thu_tiep_theo_luc
        Action=hanh_dong TaskTitle=tieu_de_nhiem_vu OldData=du_lieu_truoc NewData=du_lieu_sau ChangedAt=ngay_thay_doi
        """;
    var renamedColumns = columnPairs.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split('=')).ToDictionary(pair => pair[0], pair => pair[1], StringComparer.Ordinal);
    string[] tables = ["schools", "faculties", "cohorts", "student_classes", "users", "semesters", "courses",
        "class_sessions", "task_items", "notifications", "task_histories"];
    var vietnameseTables = new Dictionary<string, string>
    {
        ["schools"] = "truong", ["faculties"] = "khoa", ["cohorts"] = "khoa_hoc",
        ["student_classes"] = "lop_sinh_vien", ["users"] = "nguoi_dung", ["semesters"] = "hoc_ky",
        ["courses"] = "mon_hoc", ["class_sessions"] = "lich_hoc", ["task_items"] = "nhiem_vu",
        ["notifications"] = "thong_bao", ["task_histories"] = "lich_su_nhiem_vu"
    };
    const int fixtureId = 700001;
    const int childId = fixtureId + 1;
    var fixtureTime = new DateTimeOffset(2026, 10, 31, 7, 0, 0, TimeSpan.Zero);
    var fixturePassword = Guid.NewGuid().ToString("N");
    var fixtureUser = new User { Ma = fixtureId, Email = "migration-fixture@example.test" };
    var passwordHasher = new PasswordHasher<User>();
    var fixtureHash = passwordHasher.HashPassword(fixtureUser, fixturePassword);
    await using var schema = new ApplicationDbContext(options);
    var migrator = schema.GetService<IMigrator>();
    await migrator.MigrateAsync(oldMigration);
    await using var connection = new NpgsqlConnection(config.ConnectionString);
    await connection.OpenAsync();

    static string Quoted(string identifier) => "\"" + identifier.Replace("\"", "\"\"") + "\"";
    async Task InsertLegacy(string table, params (string Column, object Value)[] fields)
    {
        if (!tables.Contains(table)) throw new InvalidOperationException("Unknown fixture table.");
        if (table != "task_histories") fields = [.. fields,
            ("CreatedAt", fixtureTime.AddDays(-3)), ("UpdatedAt", fixtureTime)];
        var columns = string.Join(",", fields.Select(field => Quoted(field.Column)));
        var parameters = string.Join(",", fields.Select((_, index) => "@p" + index));
        await using var insert = new NpgsqlCommand($"INSERT INTO {Quoted(table)} ({columns}) VALUES ({parameters})", connection);
        for (var index = 0; index < fields.Length; index++)
        {
            if (fields[index].Column is "OldData" or "NewData")
                insert.Parameters.Add(new NpgsqlParameter("p" + index, NpgsqlDbType.Jsonb) { Value = fields[index].Value });
            else insert.Parameters.AddWithValue("p" + index, fields[index].Value);
        }
        await insert.ExecuteNonQueryAsync();
    }
    async Task<JsonElement[]> ReadRows(string table, bool oldColumns, bool vietnameseTable = false)
    {
        if (vietnameseTable) table = vietnameseTables[table];
        await using var query = new NpgsqlCommand($"SELECT to_jsonb(fixture)::text FROM {Quoted(table)} fixture ORDER BY fixture.{Quoted(oldColumns ? "Id" : "ma")}", connection);
        await using var reader = await query.ExecuteReaderAsync();
        var rows = new List<JsonElement>();
        while (await reader.ReadAsync())
        {
            using var row = JsonDocument.Parse(reader.GetString(0));
            rows.Add(row.RootElement.Clone());
        }
        return rows.ToArray();
    }
    static bool SameRow(JsonElement oldRow, JsonElement actual, IReadOnlyDictionary<string, string>? names, bool hasProfileNames)
    {
        var expected = oldRow.EnumerateObject().ToDictionary(property => names == null ? property.Name : names[property.Name], property => property.Value);
        var existing = actual.EnumerateObject().Where(property => !hasProfileNames || property.Name is not ("ten_truong" or "ten_lop"))
            .ToDictionary(property => property.Name, property => property.Value);
        return JsonNode.DeepEquals(JsonNode.Parse(JsonSerializer.Serialize(expected)), JsonNode.Parse(JsonSerializer.Serialize(existing)));
    }

    await InsertLegacy("schools", ("Id", fixtureId), ("Code", "MIG-SCHOOL"), ("Name", "Trường kiểm thử chuyển đổi"));
    await InsertLegacy("faculties", ("Id", fixtureId), ("SchoolId", fixtureId), ("Code", "MIG-IT"), ("Name", "Khoa Công nghệ thông tin"));
    await InsertLegacy("cohorts", ("Id", fixtureId), ("Name", "MIG-K2026"), ("StartYear", 2026), ("EndYear", 2030));
    await InsertLegacy("student_classes", ("Id", fixtureId), ("FacultyId", fixtureId), ("CohortId", fixtureId), ("Name", "MIG-CNTT01"));
    await InsertLegacy("users", ("Id", fixtureId), ("Email", fixtureUser.Email), ("NormalizedEmail", "MIGRATION-FIXTURE@EXAMPLE.TEST"),
        ("FullName", "Sinh viên dữ liệu cũ"), ("PasswordHash", fixtureHash), ("StudentCode", "MIG-SV001"), ("ClassId", fixtureId),
        ("Major", "Công nghệ thông tin"), ("Role", "Student"), ("IsActive", true), ("TimeZone", "Asia/Ho_Chi_Minh"), ("ReminderHours", 0));
    await InsertLegacy("users", ("Id", fixtureId + 2), ("Email", "migration-no-class@example.test"),
        ("NormalizedEmail", "MIGRATION-NO-CLASS@EXAMPLE.TEST"), ("FullName", "Sinh viên chưa chọn lớp"),
        ("PasswordHash", fixtureHash), ("Role", "Student"));
    await InsertLegacy("semesters", ("Id", fixtureId), ("UserId", fixtureId), ("Name", "Học kỳ chuyển đổi"),
        ("StartDate", new DateOnly(2026, 9, 1)), ("EndDate", new DateOnly(2026, 12, 31)));
    await InsertLegacy("courses", ("Id", fixtureId), ("SemesterId", fixtureId), ("Code", "MIG-CSDL"), ("Name", "Cơ sở dữ liệu"),
        ("Credits", 3), ("Lecturer", "Giảng viên thử"), ("Color", "#12aBcF"));
    await InsertLegacy("class_sessions", ("Id", fixtureId), ("CourseId", fixtureId), ("DayOfWeek", 3),
        ("StartTime", new TimeOnly(8, 15)), ("EndTime", new TimeOnly(10, 5)), ("Room", "A203"));
    await InsertLegacy("task_items", ("Id", fixtureId), ("UserId", fixtureId), ("CourseId", fixtureId), ("Title", "Bài tập cha"),
        ("Description", "Mô tả giữ nguyên dấu tiếng Việt"), ("Type", "Assignment"), ("DueDate", fixtureTime.AddDays(1)),
        ("Priority", "High"), ("Status", "Completed"), ("Progress", 100), ("ReminderHours", 0), ("CompletedAt", fixtureTime.AddHours(-2)));
    await InsertLegacy("task_items", ("Id", childId), ("UserId", fixtureId), ("CourseId", fixtureId), ("ParentTaskId", fixtureId),
        ("Title", "Phần truy vấn SQL"), ("Description", "Nội dung con"), ("Type", "Revision"), ("DueDate", fixtureTime.AddHours(-1)),
        ("Priority", "Low"), ("Status", "Completed"), ("EstimatedMinutes", 90), ("Progress", 100), ("ReminderHours", 24), ("CompletedAt", fixtureTime.AddHours(-2)));
    await InsertLegacy("notifications", ("Id", fixtureId), ("UserId", fixtureId), ("TaskId", childId), ("Type", "DeadlineReminder"),
        ("Content", "Thông báo cũ đã đọc"), ("RemindAt", fixtureTime.AddHours(-25)), ("DeliveryStatus", "Delivered"),
        ("SentAt", fixtureTime.AddHours(-25).AddMinutes(1)), ("ReadAt", fixtureTime.AddHours(-24)), ("AttemptCount", 1));
    // Các khóa JSON lịch sử cũ là dữ liệu bên trong jsonb, không được đổi bởi RenameColumn.
    await InsertLegacy("task_histories", ("Id", fixtureId), ("TaskId", childId), ("UserId", fixtureId), ("Action", "Completed"),
        ("TaskTitle", "Phần truy vấn SQL"), ("OldData", "{\"Id\":700002,\"Progress\":75,\"Status\":\"InProgress\"}"),
        ("NewData", "{\"Id\":700002,\"Progress\":100,\"Status\":\"Completed\"}"), ("ChangedAt", fixtureTime.AddHours(-2)));

    var original = new Dictionary<string, JsonElement[]>();
    foreach (var table in tables) original[table] = await ReadRows(table, oldColumns: true);
    Check(original.Values.Sum(rows => rows[0].EnumerateObject().Count()) == 98,
        "Migration fixture covers every legacy column in all eleven business tables");
    async Task AssertRows(bool oldColumns, string label, bool hasProfileNames = false)
    {
        foreach (var table in tables)
        {
            var rows = await ReadRows(table, oldColumns);
            Check(rows.Length == original[table].Length && original[table].Zip(rows).All(pair =>
                SameRow(pair.First, pair.Second, oldColumns ? null : renamedColumns, hasProfileNames && table == "users")),
                label + ": all values preserved in " + table);
        }
    }
    await migrator.MigrateAsync(renameMigration);
    await AssertRows(oldColumns: false, "Upgrade");
    await migrator.MigrateAsync(oldMigration);
    await AssertRows(oldColumns: true, "Downgrade");
    await migrator.MigrateAsync(renameMigration);
    await AssertRows(oldColumns: false, "Second upgrade");

    await migrator.MigrateAsync(profileMigration);
    await AssertRows(oldColumns: false, "Core profile upgrade", hasProfileNames: true);

    // So sánh toàn bộ 100 cột sau backfill, không bỏ qua hai tên hồ sơ mới.
    var beforeTableRename = new Dictionary<string, JsonElement[]>();
    var tableOids = new Dictionary<string, uint>();
    foreach (var table in tables)
    {
        beforeTableRename[table] = await ReadRows(table, oldColumns: false);
        await using var oid = new NpgsqlCommand("SELECT @name::regclass::oid", connection);
        oid.Parameters.AddWithValue("name", table);
        tableOids[table] = (uint)(await oid.ExecuteScalarAsync())!;
    }
    async Task<string> SequenceState()
    {
        await using var query = new NpgsqlCommand("SELECT coalesce(jsonb_agg(to_jsonb(s) ORDER BY sequencename)::text, '[]') FROM pg_sequences s WHERE schemaname='public'", connection);
        return (string)(await query.ExecuteScalarAsync())!;
    }
    var sequenceState = await SequenceState();
    async Task AssertTableRename(bool vietnamese, string label)
    {
        foreach (var table in tables)
        {
            var rows = await ReadRows(table, oldColumns: false, vietnameseTable: vietnamese);
            Check(rows.Length == beforeTableRename[table].Length && beforeTableRename[table].Zip(rows).All(pair =>
                JsonNode.DeepEquals(JsonNode.Parse(pair.First.GetRawText()), JsonNode.Parse(pair.Second.GetRawText()))),
                label + ": all values, including profile names, preserved in " + table);
            await using var oid = new NpgsqlCommand("SELECT @name::regclass::oid", connection);
            oid.Parameters.AddWithValue("name", vietnamese ? vietnameseTables[table] : table);
            Check((uint)(await oid.ExecuteScalarAsync())! == tableOids[table], label + ": same table OID for " + table);
        }
        await using var namesQuery = new NpgsqlCommand("SELECT array_agg(table_name::text) FROM information_schema.tables WHERE table_schema='public'", connection);
        var names = ((string[])(await namesQuery.ExecuteScalarAsync())!).ToHashSet();
        Check(tables.All(table => names.Contains(vietnamese ? vietnameseTables[table] : table) &&
            !names.Contains(vietnamese ? table : vietnameseTables[table])), label + ": only expected table names exist");
        Check(await SequenceState() == sequenceState, label + ": identity sequences and their state unchanged");
    }
    await migrator.MigrateAsync(tableMigration);
    await AssertTableRename(true, "Table rename upgrade");
    await migrator.MigrateAsync(profileMigration);
    await AssertTableRename(false, "Table rename downgrade");
    await migrator.MigrateAsync(tableMigration);
    await AssertTableRename(true, "Table rename second upgrade");
    foreach (var (state, offset) in new[] { ("Pending", 10), ("Failed", 11), ("Cancelled", 12) })
    {
        await using var seed = new NpgsqlCommand($"""
            INSERT INTO nhiem_vu(ma,ma_nguoi_dung,tieu_de,loai,muc_do_uu_tien,trang_thai,tien_do,ngay_tao,ngay_cap_nhat)
            VALUES({fixtureId + offset},{fixtureId},'Reminder fixture','Personal','Medium','NotStarted',0,now(),now());
            INSERT INTO thong_bao(ma,ma_nguoi_dung,ma_nhiem_vu,loai,noi_dung,thoi_diem_nhac,trang_thai_phat,so_lan_thu,lan_thu_tiep_theo_luc,ngay_tao,ngay_cap_nhat)
            VALUES({fixtureId + offset},{fixtureId},{fixtureId + offset},'DeadlineReminder','Reminder fixture',@time,@state,2,@time,now(),now());
            """, connection);
        seed.Parameters.AddWithValue("time", fixtureTime); seed.Parameters.AddWithValue("state", state);
        await seed.ExecuteNonQueryAsync();
    }
    await migrator.MigrateAsync();
    Check(await schema.Tasks.Where(x => x.Ma == childId).Select(x => x.MaHocKy).SingleAsync() == fixtureId,
        "Planning migration backfills task semester from its owned course");
    var reminder = await schema.Reminders.SingleAsync(x => x.MaThongBaoCu == fixtureId);
    Check(reminder.NhacTruocPhut == null && reminder.NgayPhat == fixtureTime.AddHours(-25).AddMinutes(1) &&
        reminder.TrangThai.ToString() == "Delivered", "Legacy reminder preserves delivered time without guessing offset");
    Check(await schema.Notifications.AnyAsync(x => x.Ma == fixtureId && x.MaLichNhac == reminder.Ma),
        "Delivered notification links to its migrated reminder");
    foreach (var state in new[] { "Pending", "Failed", "Cancelled" })
    {
        var row = await schema.Reminders.SingleAsync(x => x.TrangThai.ToString() == state);
        Check(row.ThoiDiemNhac == fixtureTime && row.SoLanThu == 2 && row.LanThuTiepTheoLuc == fixtureTime,
            "Reminder migration preserves schedule/retry for " + state);
        Check(!await schema.Notifications.AnyAsync(x => x.MaLichNhac == row.Ma), "Undelivered legacy rows do not become inbox messages");
    }
    async Task RejectSql(string sql, string state)
    {
        await using var tx = await connection.BeginTransactionAsync();
        try { await using var cmd = new NpgsqlCommand(sql, connection, tx); await cmd.ExecuteNonQueryAsync();
            throw new Exception("Unexpected valid write: " + sql); }
        catch (PostgresException ex) { Check(ex.SqlState == state, "Planning constraint rejects invalid data: " + state); }
        finally { await tx.RollbackAsync(); }
    }
    await RejectSql($"UPDATE nhiem_vu SET ma_hoc_ky=NULL WHERE ma={childId}", "23514");
    await RejectSql($"UPDATE nhiem_vu SET ma_nguoi_dung={fixtureId + 2} WHERE ma={childId}", "23503");
    await RejectSql($"UPDATE nhiem_vu SET bat_dau_su_kien=now() WHERE ma={childId}", "23514");
    await RejectSql($"INSERT INTO phien_tu_hoc(ma_nguoi_dung,ma_nhiem_vu,bat_dau_du_kien,ket_thuc_du_kien,trang_thai,ngay_tao,ngay_cap_nhat) VALUES({fixtureId + 2},{childId},now(),now()+interval '1 hour','Planned',now(),now())", "23503");
    await RejectSql($"INSERT INTO phien_tu_hoc(ma_nguoi_dung,ma_nhiem_vu,bat_dau_du_kien,ket_thuc_du_kien,trang_thai,ngay_tao,ngay_cap_nhat) VALUES({fixtureId},{childId},now(),now(),'Planned',now(),now())", "23514");
    await RejectSql($"INSERT INTO tep_dinh_kem_nhiem_vu(ma_nhiem_vu,ma_nguoi_tai_len,ten_tep,khoa_luu_tru,loai_tep,kich_thuoc_byte,ngay_tao) VALUES({childId},{fixtureId + 2},'a.pdf','test/a','application/pdf',10,now())", "23503");
    await RejectSql($"INSERT INTO lan_ap_dung_lich(ma_nguoi_dung,khoa_yeu_cau,dau_kiem_noi_dung,phien_ban_nguon,phien_ban_quy_tac,ngay_tao) VALUES({fixtureId},'same',repeat('a',64),0,1,now()),({fixtureId},'same',repeat('b',64),0,1,now())", "23505");
    await RejectSql($"INSERT INTO doan_thuc_hoc(ma_nguoi_dung,ma_nhiem_vu,bat_dau_thuc_te) VALUES({fixtureId},{childId},now()),({fixtureId},{childId},now())", "23505");
    await RejectSql($"INSERT INTO khung_gio_ranh(ma_nguoi_dung,thu_trong_tuan,gio_bat_dau,gio_ket_thuc,ngay_bat_dau,ngay_ket_thuc,dang_su_dung,ngay_tao,ngay_cap_nhat) VALUES({fixtureId},8,'20:00','19:00','2026-10-01','2026-10-31',true,now(),now())", "23514");
    // Model hiện tại chỉ đọc EF sau khi cả cột hồ sơ và tên bảng đã nâng cấp.
    var migrated = await schema.Users.AsNoTracking().SingleAsync(user => user.Ma == fixtureId);
    Check(migrated.TenTruong == "Trường kiểm thử chuyển đổi" && migrated.TenLop == "MIG-CNTT01" && migrated.MaLop == fixtureId,
        "Profile names are backfilled from the legacy school/class without dropping the legacy FK");
    var unlinked = await schema.Users.AsNoTracking().SingleAsync(user => user.Ma == fixtureId + 2);
    Check(unlinked.TenTruong == null && unlinked.TenLop == null && unlinked.MaLop == null,
        "Users without a legacy class keep optional profile names empty");
    Check(migrated.MatKhauBam == fixtureHash && passwordHasher.VerifyHashedPassword(migrated, migrated.MatKhauBam, fixturePassword) != PasswordVerificationResult.Failed,
        "Legacy password hash remains byte-identical and still verifies after upgrade");
    Check(await schema.Tasks.AsNoTracking().AnyAsync(task => task.Ma == childId && task.ParentTask!.Ma == fixtureId &&
        task.Course!.Semester.User.Ma == fixtureId && task.User.Class!.Faculty.School.Ma == fixtureId),
        "Renamed ownership, task tree, course and catalog foreign keys remain readable");
    // Chỉ fixture trong DB tên ngẫu nhiên do lượt test này tạo; không đụng cấu hình appsettings/Neon.
    var planningTables = new[] { "lich_nhac", "phien_tu_hoc", "khung_gio_ranh", "lan_ap_dung_lich", "doan_thuc_hoc", "tep_dinh_kem_nhiem_vu", "quy_tac_uu_tien" };
    await using var cleanup = new NpgsqlCommand("TRUNCATE TABLE " + string.Join(",", tables.Select(table => Quoted(vietnameseTables[table])).Concat(planningTables.Select(Quoted))) + " RESTART IDENTITY", connection);
    await cleanup.ExecuteNonQueryAsync();
    Check(!await schema.Users.AnyAsync() && !await schema.Tasks.AnyAsync() && !await schema.TaskHistories.AnyAsync(),
        "Migration fixture removed before the HTTP authentication scenarios");
}
try
{
    await VerifyColumnRenameMigrationAsync();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
    builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
    builder.Services.AddAdministration(); builder.Services.AddStudyPlannerAuth(builder.Configuration, builder.Environment);
    builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(o => {
        o.TagActionsBy(api => [VietnameseSwagger.Group(api.ActionDescriptor.RouteValues["controller"] ?? "")]);
        o.OperationFilter<VietnameseSwagger>(); o.SchemaFilter<VietnameseSwagger>();
    });
    app = builder.Build(); app.UseExceptionHandler(); app.UseSwagger();
    // Chỉ fixture test: mỗi ca chức năng dùng IP riêng để không chạm quota trong cùng một giây.
    // Các request cuối kiểm tra rate limit không có header này và vẫn dùng cùng IP localhost thật.
    var rateBucket = 0;
    app.Use(async (context, next) => {
        if (context.Request.Headers.ContainsKey("X-Test-Isolate-Rate"))
        {
            var bucket = Interlocked.Increment(ref rateBucket);
            context.Connection.RemoteIpAddress = IPAddress.Parse($"127.20.{bucket / 250}.{bucket % 250 + 1}");
        }
        await next();
    });
    app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
    using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
    async Task<JsonElement> Send(HttpMethod method, string path, object? body, int status)
    {
        using var req = new HttpRequestMessage(method, path);
        req.Headers.Add("X-Test-Isolate-Rate", "1");
        if (body != null) req.Content = JsonContent.Create(body);
        using var res = await client.SendAsync(req);
        Check((int)res.StatusCode == status, $"{method} {path} expected {status}, received {(int)res.StatusCode}");
        if (status == 201) Check(res.Headers.Location?.OriginalString == "/api/v1/users/me", "Registration Location uses the v1 profile route");
        if (status >= 400) Check(res.Content.Headers.ContentType?.MediaType == "application/problem+json", "Safe errors");
        var content = await res.Content.ReadAsStringAsync();
        return content.Length > 0 ? JsonDocument.Parse(content).RootElement.Clone() : default;
    }
    async Task AssertRetiredRoutes(string actor)
    {
        foreach (var resource in new[] { "schools", "faculties", "cohorts", "student-classes" })
        {
            foreach (var (method, suffix) in new[] { (HttpMethod.Get, ""), (HttpMethod.Get, "/1"),
                (HttpMethod.Post, ""), (HttpMethod.Put, "/1"), (HttpMethod.Delete, "/1") })
            {
                using var request = new HttpRequestMessage(method, "/api/v1/admin/" + resource + suffix)
                    { Content = JsonContent.Create(new { }) };
                using var response = await client.SendAsync(request);
                Check(response.StatusCode == HttpStatusCode.NotFound, actor + ": retired catalog route remains absent: " + method + " " + resource + suffix);
            }
            using var lookup = await client.GetAsync("/api/v1/catalogs/" + resource);
            Check(lookup.StatusCode == HttpStatusCode.NotFound, actor + ": retired catalog lookup remains absent: " + resource);
        }
    }
    await AssertRetiredRoutes("Anonymous");
    var password = Guid.NewGuid().ToString("N");
    JsonObject Registration() => new()
    {
        ["email"] = "validation@example.test", ["hoTen"] = "Sinh viên", ["matKhau"] = password,
        ["maSoSinhVien"] = "0306000099", ["tenTruong"] = "Trường thử", ["tenLop"] = "CNTT01", ["nganhHoc"] = "IT"
    };
    foreach (var field in new[] { "email", "hoTen", "matKhau", "maSoSinhVien", "tenTruong", "tenLop", "nganhHoc" })
    {
        var missing = Registration(); missing.Remove(field);
        await Send(HttpMethod.Post, "/api/v1/auth/register", missing, 400);
        var blank = Registration(); blank[field] = "   ";
        await Send(HttpMethod.Post, "/api/v1/auth/register", blank, 400);
        var nil = Registration(); nil[field] = null;
        await Send(HttpMethod.Post, "/api/v1/auth/register", nil, 400);
    }
    foreach (var (field, maximum) in new[] { ("maSoSinhVien", 50), ("tenTruong", 200), ("tenLop", 200), ("nganhHoc", 200) })
    {
        var tooLong = Registration(); tooLong[field] = new string('x', maximum + 1);
        await Send(HttpMethod.Post, "/api/v1/auth/register", tooLong, 400);
    }
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = " ", matKhau = password }, 400);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { email = "first@example.test", matKhau = password }, 400);
    var first = await Send(HttpMethod.Post, "/api/v1/auth/register", new { email = "first@example.test", hoTen = "First", matKhau = password, maSoSinhVien = " 0306000001 ", tenTruong = " Trường thử ", tenLop = " CNTT01 ", nganhHoc = " Công nghệ thông tin ", vaiTro = "Admin", dangHoatDong = false }, 201);
    var id = first.GetProperty("ma").GetInt32();
    Check(!first.TryGetProperty("id", out _) && !first.TryGetProperty("fullName", out _) && !first.TryGetProperty("maLop", out _),
        "Profile uses Vietnamese JSON fields without a legacy class ID");
    Check(first.GetProperty("maSoSinhVien").GetString() == "0306000001" &&
        first.GetProperty("tenTruong").GetString() == "Trường thử" && first.GetProperty("tenLop").GetString() == "CNTT01" &&
        first.GetProperty("nganhHoc").GetString() == "Công nghệ thông tin",
        "Registration persists required profile, trims names and preserves leading student-code zeros");
    Check(first.GetProperty("vaiTro").GetString() == "Student" && first.GetProperty("dangHoatDong").GetBoolean(), "Register cannot set role/active");
    Check(!first.TryGetProperty("matKhauBam", out _), "No hash response");
    await Send(HttpMethod.Post, "/api/v1/auth/register", new { email = " FIRST@EXAMPLE.TEST ", hoTen = "Duplicate", matKhau = password, maSoSinhVien = "0306000003", tenTruong = "Trường thử", tenLop = "CNTT01", nganhHoc = "IT" }, 409);
    var duplicateCode = Registration(); duplicateCode["maSoSinhVien"] = "0306000001";
    await Send(HttpMethod.Post, "/api/v1/auth/register", duplicateCode, 409);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "first@example.test", matKhau = "incorrect" }, 401);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "missing@example.test", matKhau = password }, 401);
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    await Send(HttpMethod.Get, "/api/v1/admin/users", null, 401);
    var login = await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = " FIRST@EXAMPLE.TEST ", matKhau = password }, 200);
    var byCode = await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = " 0306000001 ", matKhau = password }, 200);
    Check(byCode.GetProperty("nguoiDung").GetProperty("ma").GetInt32() == id, "Email and student-code login identify the same user");
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "0306000001", matKhau = "wrong" }, 401);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "306000001", matKhau = password }, 401);
    var token = login.GetProperty("tokenTruyCap").GetString()!;
    Check(login.GetProperty("nguoiDung").GetProperty("ma").GetInt32() == id &&
        login.GetProperty("loaiToken").GetString() == "Bearer" && login.GetProperty("hetHanLuc").GetDateTimeOffset() > DateTimeOffset.UtcNow,
        "Login response has Vietnamese token, expiry and account fields");
    Check(!login.TryGetProperty("accessToken", out _) && !login.TryGetProperty("user", out _), "Login omits legacy English field aliases");
    var jwtClaims = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token).Claims.ToArray();
    Check(jwtClaims.Any(claim => claim.Type == "sub" && claim.Value == id.ToString()) &&
        jwtClaims.Any(claim => claim.Type == "role" && claim.Value == "Student") &&
        jwtClaims.Any(claim => claim.Type == "pwd") && jwtClaims.Any(claim => claim.Type == "jti"),
        "Renaming business fields preserves the JWT claim contract");
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    var me = await Send(HttpMethod.Get, "/api/v1/users/me", null, 200);
    Check(me.GetProperty("ma").GetInt32() == id, "Own identity from JWT");
    await AssertRetiredRoutes("Student");
    await Send(HttpMethod.Get, "/api/v1/admin/users", null, 403);
    foreach (var action in new[] { "activate", "deactivate" })
        await Send(HttpMethod.Patch, $"/api/v1/admin/users/{first.GetProperty("ma").GetInt32()}/{action}", null, 403);
    await Send(HttpMethod.Post, "/api/v1/auth/register", new { email = "second@example.test", hoTen = "Second", matKhau = password, maSoSinhVien = "0306000002", tenTruong = "Trường thử", tenLop = "CNTT02", nganhHoc = "IT" }, 201);
    await using (var db = new ApplicationDbContext(options))
    {
        var second = await db.Users.SingleAsync(x => x.EmailChuanHoa == "SECOND@EXAMPLE.TEST");
        second.MaSoSinhVien = "first@example.test"; await db.SaveChangesAsync();
    }
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "first@example.test", matKhau = password }, 401);
    await using (var db = new ApplicationDbContext(options))
    {
        var second = await db.Users.SingleAsync(x => x.EmailChuanHoa == "SECOND@EXAMPLE.TEST");
        second.MaSoSinhVien = null; await db.SaveChangesAsync();
    }
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "second@example.test", matKhau = password }, 200);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "0306000002", matKhau = password }, 401);
    var other = await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "second@example.test", matKhau = password }, 200);
    client.DefaultRequestHeaders.Authorization = new("Bearer", other.GetProperty("tokenTruyCap").GetString());
    me = await Send(HttpMethod.Get, "/api/v1/users/me", null, 200);
    Check(me.GetProperty("ma").GetInt32() != id, "Second account reads its own profile");
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    await Send(HttpMethod.Patch, "/api/v1/users/me", new { hoTen = "Updated", nganhHoc = "IT", tenTruong = "  Trường tự khai  ", tenLop = "  CNTT01  " }, 200);
    me = await Send(HttpMethod.Patch, "/api/v1/users/me", new { nganhHoc = (string?)null }, 200);
    Check(me.GetProperty("nganhHoc").ValueKind == JsonValueKind.Null && me.GetProperty("hoTen").GetString() == "Updated" &&
        me.GetProperty("tenTruong").GetString() == "Trường tự khai" && me.GetProperty("tenLop").GetString() == "CNTT01", "PATCH omitted vs null and trimmed profile names");
    me = await Send(HttpMethod.Patch, "/api/v1/users/me", new { tenLop = (string?)null }, 200);
    Check(me.GetProperty("tenLop").ValueKind == JsonValueKind.Null && me.GetProperty("tenTruong").GetString() == "Trường tự khai" &&
        !me.TryGetProperty("maLop", out _), "PATCH clears self-declared class name without exposing legacy class links");
    await Send(HttpMethod.Patch, "/api/v1/users/me", new { vaiTro = "Admin" }, 400);
    await Send(HttpMethod.Patch, "/api/v1/users/me", new { maLop = 1 }, 400);
    await Send(HttpMethod.Patch, "/api/v1/users/me", new { tenTruong = new string('a', 201) }, 400);
    await Send(HttpMethod.Patch, "/api/v1/users/me", new { tenLop = true }, 400);
    await using (var db = new ApplicationDbContext(options)) { (await db.Users.FindAsync(id))!.DangHoatDong = false; await db.SaveChangesAsync(); }
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "first@example.test", matKhau = password }, 401);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "0306000001", matKhau = password }, 401);
    await using (var db = new ApplicationDbContext(options)) { var u = (await db.Users.FindAsync(id))!; u.DangHoatDong = true; u.VaiTro = UserRole.Admin; await db.SaveChangesAsync(); }
    await Send(HttpMethod.Get, "/api/v1/admin/users", null, 200); // Quyền cập nhật từ DB, không phải claim cũ.
    await AssertRetiredRoutes("Admin");
    await using (var db = new ApplicationDbContext(options)) { (await db.Users.FindAsync(id))!.VaiTro = UserRole.Student; await db.SaveChangesAsync(); }
    await Send(HttpMethod.Get, "/api/v1/admin/users", null, 403);
    var nextPassword = Guid.NewGuid().ToString("N");
    await Send(HttpMethod.Post, "/api/v1/users/me/change-password", new { matKhauHienTai = "wrong", matKhauMoi = nextPassword }, 401);
    var changedPassword = await Send(HttpMethod.Post, "/api/v1/users/me/change-password", new { matKhauHienTai = password, matKhauMoi = nextPassword }, 200);
    Check(changedPassword.GetProperty("thongBao").GetString() == "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.",
        "Password change returns an explicit success message");
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "first@example.test", matKhau = password }, 401);
    login = await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "first@example.test", matKhau = nextPassword }, 200);
    await Send(HttpMethod.Post, "/api/v1/auth/login", new { taiKhoan = "0306000001", matKhau = nextPassword }, 200);
    client.DefaultRequestHeaders.Authorization = new("Bearer", login.GetProperty("tokenTruyCap").GetString());
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 200);
    await using (var db = new ApplicationDbContext(options)) { db.Users.Remove((await db.Users.FindAsync(id))!); await db.SaveChangesAsync(); }
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    client.DefaultRequestHeaders.Authorization = new("Bearer", token[..^4] + "bad!");
    await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    // Token hết hạn/ký sai/issuer sai được tạo bởi thư viện chuẩn, không tự mock validator.
    User validatorUser;
    await using (var db = new ApplicationDbContext(options)) validatorUser = await db.Users.SingleAsync(x => x.EmailChuanHoa == "SECOND@EXAMPLE.TEST");
    var stamp = app.Services.GetRequiredService<JwtTokenIssuer>().Stamp(validatorUser);
    foreach (var variant in new[] { "expired", "issuer", "audience", "signature" })
    {
        var key = variant == "signature" ? RandomNumberGenerator.GetBytes(64) : System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Auth:SigningKey"]!);
        var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(variant == "issuer" ? "Wrong" : "StudyPlanner",
            variant == "audience" ? "Wrong" : "StudyPlanner.Client", [new System.Security.Claims.Claim("sub", validatorUser.Ma.ToString()), new System.Security.Claims.Claim("pwd", stamp)],
            DateTime.UtcNow.AddHours(-2), variant == "expired" ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddMinutes(5),
            new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
        client.DefaultRequestHeaders.Authorization = new("Bearer", new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwt));
        await Send(HttpMethod.Get, "/api/v1/users/me", null, 401);
    }
    client.DefaultRequestHeaders.Authorization = null;
    // Không giữ alias cũ: client phải gọi đúng hợp đồng v1, v2 chưa được hỗ trợ.
    foreach (var (method, path) in new[] {
        (HttpMethod.Post, "/api/auth/register"), (HttpMethod.Post, "/api/auth/login"),
        (HttpMethod.Get, "/api/users/me"), (HttpMethod.Get, "/api/admin/users"),
        (HttpMethod.Post, "/api/v2/auth/login") })
    {
        using var oldRequest = new HttpRequestMessage(method, path);
        using var oldResponse = await client.SendAsync(oldRequest);
        Check(oldResponse.StatusCode == HttpStatusCode.NotFound, "Unmapped API version returns 404: " + path);
    }
    var openapi = await Send(HttpMethod.Get, "/swagger/v1/swagger.json", null, 200);
    Check(openapi.GetProperty("paths").EnumerateObject().All(p => !p.Name.StartsWith("/api/", StringComparison.Ordinal) ||
        p.Name.StartsWith("/api/v1/", StringComparison.Ordinal)), "Every documented business endpoint uses v1");
    Check(openapi.GetProperty("paths").TryGetProperty("/health/live", out _), "Health remains outside the business API version");
    var schemas = openapi.GetProperty("components").GetProperty("schemas");
    foreach (var action in new[] { "activate", "deactivate" })
    {
        var toggleOperation = openapi.GetProperty("paths").GetProperty("/api/v1/admin/users/{id}/" + action).GetProperty("patch");
        Check(!toggleOperation.TryGetProperty("requestBody", out _) && toggleOperation.GetProperty("responses").TryGetProperty("200", out _), "Activation operation needs no body and documents 200: " + action);
    }
    var passwordResponses = openapi.GetProperty("paths").GetProperty("/api/v1/users/me/change-password").GetProperty("post").GetProperty("responses");
    Check(passwordResponses.TryGetProperty("200", out var passwordOk) && passwordOk.TryGetProperty("content", out _) &&
        !passwordResponses.TryGetProperty("204", out _), "OpenAPI documents password change as 200 with a response body");
    var required = schemas.GetProperty("RegisterRequest").GetProperty("required").EnumerateArray().Select(x => x.GetString()).ToArray();
    Check(new[] { "email", "hoTen", "matKhau", "maSoSinhVien", "tenTruong", "tenLop", "nganhHoc" }.All(required.Contains),
        "OpenAPI requires every registration field");
    var loginProperties = schemas.GetProperty("LoginRequest").GetProperty("properties");
    Check(loginProperties.TryGetProperty("taiKhoan", out _) && !loginProperties.TryGetProperty("email", out _),
        "Swagger login exposes taiKhoan, not the old email request field");
    Check(schemas.GetProperty("RegisterRequest").GetProperty("example").GetProperty("maSoSinhVien").GetString() == "0306000001",
        "Swagger provides a usable registration example with leading zeroes");
    for (var i = 0; i < 22; i++) { using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { taiKhoan = "missing@example.test", matKhau = password });
        if (response.StatusCode == HttpStatusCode.TooManyRequests) { Check(true, "Login rate limit"); break; }
        if (i == 21) throw new Exception("Rate limit missing"); }
    Console.WriteLine($"All {checks} Auth PostgreSQL/HTTP checks passed.");
}
finally
{
    if (app != null) { await app.StopAsync(); await app.DisposeAsync(); }
    NpgsqlConnection.ClearAllPools();
    await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
}
