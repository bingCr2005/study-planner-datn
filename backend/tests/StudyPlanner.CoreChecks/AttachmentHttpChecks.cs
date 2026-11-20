using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyPlanner.Api.Administration;
using StudyPlanner.Api.Auth;
using StudyPlanner.Api.Controllers;
using StudyPlanner.Api.HocTap;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Infrastructure.Data;
using StudyPlanner.Infrastructure.TepDinhKem;

namespace StudyPlanner.CoreChecks;

public static class AttachmentHttpChecks
{
    public static async Task RunAsync(string supplied)
    {
        var config = new NpgsqlConnectionStringBuilder(supplied);
        if (config.Host is not ("localhost" or "127.0.0.1") || config.Database != "postgres")
            throw new InvalidOperationException("Chỉ dùng PostgreSQL localhost biệt lập cho kiểm thử tệp.");
        var name = "studyplanner_attachments_" + Guid.NewGuid().ToString("N");
        var folder = Path.Combine(Path.GetTempPath(), name);
        await using var admin = new NpgsqlConnection(config.ConnectionString); await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin)) await create.ExecuteNonQueryAsync();
        config.Database = name;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(config.ConnectionString).Options;
        WebApplication? app = null; var checks = 0;
        void Check(bool ok, string text) { if (!ok) throw new Exception("Tệp đính kèm: " + text); checks++; }
        try
        {
            NguoiDung owner, other; NhiemVu task;
            await using (var db = new ApplicationDbContext(options))
            {
                await db.Database.MigrateAsync();
                NguoiDung User(string email) => new() { Email = email, EmailChuanHoa = email.ToUpperInvariant(), HoTen = "Kiểm thử tệp", MatKhauBam = "fixture-hash" };
                owner = User("tep@example.test"); other = User("khac@example.test"); db.AddRange(owner, other); await db.SaveChangesAsync();
                task = new() { MaNguoiDung = owner.Ma, TieuDe = "Báo cáo đồ án" }; db.Add(task); await db.SaveChangesAsync();
            }
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration["Auth:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(config.ConnectionString));
            builder.Services.AddQuanTri(); builder.Services.AddHocTap(); builder.Services.AddStudyPlannerXacThuc(builder.Configuration, builder.Environment);
            builder.Services.AddControllers().AddApplicationPart(typeof(TepDinhKemController).Assembly);
            var kho = new KhoTepNhiemVu(folder); builder.Services.AddSingleton(kho); builder.Services.AddScoped<TepDinhKemService>();
            app = builder.Build(); app.UseExceptionHandler(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter(); app.MapControllers(); await app.StartAsync();
            var url = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new(url) }; using var stranger = new HttpClient { BaseAddress = new(url) };
            using var anonymous = new HttpClient { BaseAddress = new(url) };
            var issuer = app.Services.GetRequiredService<JwtTokenIssuer>();
            client.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(owner).TokenTruyCap);
            stranger.DefaultRequestHeaders.Authorization = new("Bearer", issuer.Issue(other).TokenTruyCap);
            var path = $"/api/v1/tasks/{task.Ma}/attachments"; long version = 0;
            async Task<HttpResponseMessage> Send(HttpClient who, HttpMethod method, string route, int status = 200, HttpContent? body = null, long? expected = null)
            {
                using var request = new HttpRequestMessage(method, route) { Content = body };
                if (expected.HasValue) request.Headers.TryAddWithoutValidation("If-Match", $"\"ht-{expected}\"");
                var r = await who.SendAsync(request);
                Check((int)r.StatusCode == status, $"{method} {route}: {r.StatusCode}, mong đợi {status}");
                if (r.IsSuccessStatusCode && r.Headers.ETag?.Tag is string etag) version = long.Parse(etag[4..^1]);
                return r;
            }
            MultipartFormDataContent Form(byte[] bytes, string ten = "Báo cáo.pdf")
            {
                var form = new MultipartFormDataContent(); form.Add(new ByteArrayContent(bytes), "tep", ten); return form;
            }
            using (await Send(anonymous, HttpMethod.Get, path, 401)) { }
            using (await Send(stranger, HttpMethod.Get, path, 404)) { }
            using (var r = await Send(client, HttpMethod.Get, path)) Check((await r.Content.ReadAsStringAsync()).Contains("\"duLieu\":[]"), "Ban đầu chưa có tệp");
            var bytes = new byte[] { 37, 80, 68, 70, 0, 255, 42 };
            using (await Send(client, HttpMethod.Post, path, 428, Form(bytes))) { }
            using (await Send(client, HttpMethod.Post, path, 400, Form([]), version)) { }
            using (await Send(client, HttpMethod.Post, path, 400, Form(new byte[10 * 1024 * 1024 + 1]), version)) { }
            int file;
            using (var r = await Send(client, HttpMethod.Post, path, 201, Form(bytes, "../../Báo cáo.pdf"), version))
            {
                var json = JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement;
                var data = json.GetProperty("duLieu"); file = data.GetProperty("ma").GetInt32();
                Check(data.GetProperty("tenTep").GetString() == "Báo cáo.pdf" && !data.TryGetProperty("khoaLuuTru", out _), "Ẩn khóa kho và loại đường dẫn khỏi tên");
                Check(version == 1, "Tải lên tăng phiên bản một lần");
            }
            using (await Send(client, HttpMethod.Post, path, 409, Form(bytes), 0)) { }
            using (await Send(stranger, HttpMethod.Get, path + $"/{file}/download", 404)) { }
            using (await Send(stranger, HttpMethod.Delete, path + $"/{file}", 404, expected: 0)) { }
            using (var r = await Send(client, HttpMethod.Get, path + $"/{file}/download"))
            {
                Check((await r.Content.ReadAsByteArrayAsync()).SequenceEqual(bytes), "Tải xuống giữ nguyên từng byte");
                Check(r.Content.Headers.ContentDisposition?.DispositionType == "attachment" && r.Headers.GetValues("X-Content-Type-Options").Single() == "nosniff", "Luôn tải xuống và không suy diễn MIME");
            }
            // Metadata giữ nguyên trong thùng rác để có thể khôi phục; API không cho truy cập tệp đã xóa mềm.
            await using (var db = new ApplicationDbContext(options)) { await db.DanhSachNhiemVu.Where(x => x.Ma == task.Ma).ExecuteUpdateAsync(s => s.SetProperty(x => x.NgayXoa, DateTimeOffset.UtcNow)); }
            using (await Send(client, HttpMethod.Get, path, 404)) { }
            using (await Send(client, HttpMethod.Get, path + $"/{file}/download", 404)) { }
            using (await Send(client, HttpMethod.Post, path, 404, Form(bytes), version)) { }
            await using (var db = new ApplicationDbContext(options)) { await db.DanhSachNhiemVu.IgnoreQueryFilters().Where(x => x.Ma == task.Ma).ExecuteUpdateAsync(s => s.SetProperty(x => x.NgayXoa, (DateTimeOffset?)null)); }
            using (await Send(client, HttpMethod.Get, path + $"/{file}/download")) { }
            using (await Send(client, HttpMethod.Delete, path + $"/{file}", 409, expected: 0)) { }
            using (await Send(client, HttpMethod.Delete, path + $"/{file}", 204, expected: version)) { }
            Check(!Directory.EnumerateFiles(folder).Any(), "Xóa tệp dọn cả nội dung");
            var historyPath = $"/api/v1/task-history?maNhiemVu={task.Ma}";
            using (var r = await Send(client, HttpMethod.Get, historyPath))
            using (var json = JsonDocument.Parse(await r.Content.ReadAsStringAsync()))
            {
                var rows = json.RootElement.GetProperty("duLieu").GetProperty("danhSach");
                Check(rows.GetArrayLength() == 2, "Chỉ thêm/xóa thành công tạo lịch sử, tải xuống và thao tác thất bại không tạo lịch sử");
                using var removed = JsonDocument.Parse(rows[0].GetProperty("duLieuSau").GetString()!);
                using var added = JsonDocument.Parse(rows[1].GetProperty("duLieuSau").GetString()!);
                Check(removed.RootElement.GetProperty("loaiHoatDong").GetString() == "AttachmentRemoved" &&
                    added.RootElement.GetProperty("loaiHoatDong").GetString() == "AttachmentAdded" &&
                    new[] { added.RootElement, removed.RootElement }.All(x => x.GetProperty("tenTep").GetString() == "Báo cáo.pdf" && x.EnumerateObject().Count() == 2),
                    "Lịch sử giữ đúng tên hiển thị và hành động, không lưu khóa kho hoặc nội dung");
            }
            using (var r = await Send(stranger, HttpMethod.Get, historyPath))
            using (var json = JsonDocument.Parse(await r.Content.ReadAsStringAsync()))
                Check(json.RootElement.GetProperty("duLieu").GetProperty("danhSach").GetArrayLength() == 0, "Tài khoản khác không thấy lịch sử tệp");
            using (await Send(client, HttpMethod.Get, path)) { } // Lấy lại version của chủ sau khi dùng token của tài khoản khác.
            for (int i = 0; i < 20; i++) using (await Send(client, HttpMethod.Post, path, 201, Form(bytes), version)) { }
            using (await Send(client, HttpMethod.Post, path, 400, Form(bytes), version)) { }
            // Mô phỏng một tệp cũ chưa COMMIT và các tệp của nhiệm vụ đã xóa vĩnh viễn.
            var orphan = await kho.GhiAsync(new MemoryStream(bytes), bytes.Length, default);
            File.SetLastWriteTimeUtc(Path.Combine(folder, orphan), DateTime.UtcNow.AddDays(-2));
            var worker = new DonKhoTepWorker(app.Services.GetRequiredService<IServiceScopeFactory>(), kho, TimeProvider.System,
                app.Services.GetRequiredService<ILogger<DonKhoTepWorker>>());
            await worker.DonAsync(default);
            Check(Directory.EnumerateFiles(folder).Count() == 20, "Chỉ dọn tệp mồ côi cũ, giữ tệp còn metadata");
            foreach (var f in Directory.EnumerateFiles(folder)) File.SetLastWriteTimeUtc(f, DateTime.UtcNow.AddDays(-2));
            await using (var db = new ApplicationDbContext(options)) { db.Remove(await db.DanhSachNhiemVu.SingleAsync(x => x.Ma == task.Ma)); await db.SaveChangesAsync(); Check(!await db.DanhSachTepDinhKemNhiemVu.IgnoreQueryFilters().AnyAsync(), "Cascade xóa metadata theo nhiệm vụ"); }
            await worker.DonAsync(default); Check(!Directory.EnumerateFiles(folder).Any(), "Dọn nội dung sau xóa vĩnh viễn");
            Console.WriteLine($"PASS: {checks} kiểm tra HTTP/PostgreSQL/kho tệp đính kèm.");
        }
        finally
        {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            NpgsqlConnection.ClearAllPools();
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{name}\" WITH (FORCE)", admin); await drop.ExecuteNonQueryAsync();
            // Thư mục có tên ngẫu nhiên do chính bài test tạo trong temp.
            if (Path.GetFullPath(folder) != Path.Combine(Path.GetFullPath(Path.GetTempPath()), name))
                throw new InvalidOperationException("Thư mục dọn kiểm thử nằm ngoài vùng temp dự kiến.");
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }
}
