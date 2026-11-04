using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
using StudyPlanner.Infrastructure.Data;

// Only build metadata and SQL: never connect to or change a real database.
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql("Host=localhost;Database=schema_checks;Username=unused;Password=unused")
    .Options;
using var context = new ApplicationDbContext(options);
var model = context.GetService<IDesignTimeModel>().Model;
var entities = model.GetEntityTypes().ToArray();
var expectedTables = new[]
{
    "truong", "khoa", "khoa_hoc", "lop_sinh_vien", "nguoi_dung",
    "hoc_ky", "mon_hoc", "lich_hoc", "nhiem_vu", "thong_bao", "lich_su_nhiem_vu", "khung_gio_ranh", "phien_tu_hoc", "lich_nhac", "tep_dinh_kem_nhiem_vu", "lan_ap_dung_lich", "doan_thuc_hoc", "quy_tac_uu_tien"
};
Check(entities.Select(x => x.GetTableName()).Order().SequenceEqual(expectedTables.Order()),
    "All eighteen tables use Vietnamese names, including seven new planning tables");
var tableMigration = new StudyPlanner.Infrastructure.Migrations.RenameBusinessTablesToVietnamese();
Check(tableMigration.UpOperations.Count == 11 && tableMigration.UpOperations.All(x => x is RenameTableOperation),
    "Table migration upgrade contains only eleven RenameTable operations; no data or schema recreation");
Check(tableMigration.DownOperations.Count == 11 && tableMigration.DownOperations.All(x => x is RenameTableOperation),
    "Table migration downgrade only restores table names");
Check(entities.Sum(x => x.GetForeignKeys().Count()) == 27, "27 foreign keys including owned planning data");
Check(entities.All(x => x.GetProperties().All(p => !p.IsShadowProperty())),
    "No accidental shadow columns from misspelled foreign keys");
Check(entities.All(entity => entity.GetProperties().All(property =>
    property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema())) ==
    System.Text.RegularExpressions.Regex.Replace(property.Name, "(?<!^)([A-Z])", "_$1").ToLowerInvariant())),
    "Vietnamese C# properties map to their snake_case PostgreSQL columns");

var expectedDeletes = new (Type Dependent, string Property, Type Principal, DeleteBehavior Behavior)[]
{
    (typeof(Khoa), "MaTruong", typeof(Truong), DeleteBehavior.Restrict),
    (typeof(LopSinhVien), "MaKhoa", typeof(Khoa), DeleteBehavior.Restrict),
    (typeof(LopSinhVien), "MaKhoaHoc", typeof(KhoaHoc), DeleteBehavior.Restrict),
    (typeof(NguoiDung), "MaLop", typeof(LopSinhVien), DeleteBehavior.SetNull),
    (typeof(HocKy), "MaNguoiDung", typeof(NguoiDung), DeleteBehavior.Cascade),
    (typeof(MonHoc), "MaHocKy", typeof(HocKy), DeleteBehavior.Cascade),
    (typeof(LichHoc), "MaMonHoc", typeof(MonHoc), DeleteBehavior.Cascade),
    (typeof(NhiemVu), "MaNguoiDung", typeof(NguoiDung), DeleteBehavior.Cascade),

    (typeof(NhiemVu), "MaNhiemVuCha", typeof(NhiemVu), DeleteBehavior.Cascade),
    (typeof(ThongBao), "MaNguoiDung", typeof(NguoiDung), DeleteBehavior.Cascade),
    (typeof(ThongBao), "MaNhiemVu", typeof(NhiemVu), DeleteBehavior.Cascade),
    (typeof(LichSuNhiemVu), "MaNguoiDung", typeof(NguoiDung), DeleteBehavior.SetNull)
};
foreach (var (dependent, property, principal, behavior) in expectedDeletes)
{
    var fk = Entity(dependent).GetForeignKeys().Single(x =>
        x.Properties.Count == 1 && x.Properties[0].Name == property);
    Check(fk.PrincipalEntityType.ClrType == principal && fk.DeleteBehavior == behavior,
        dependent.Name + "." + property + " delete behavior");
}
Check(Entity(typeof(LichSuNhiemVu)).GetForeignKeys().All(x =>
    x.Properties.All(p => p.Name != "MaNhiemVu")), "History survives task deletion without a Task FK");
Check(Entity(typeof(LichSuNhiemVu)).FindProperty("DuLieuTruoc")!.GetColumnType() == "jsonb",
    "History snapshots use jsonb");
Check(Entity(typeof(NhiemVu)).FindProperty("MaMonHoc")!.IsNullable &&
      Entity(typeof(NhiemVu)).FindProperty("MaNhiemVuCha")!.IsNullable, "Personal and root tasks allowed");
Check(Entity(typeof(HocKy)).FindProperty("NgayBatDau")!.GetColumnType() == "date" &&
      Entity(typeof(LichHoc)).FindProperty("GioBatDau")!.GetColumnType() == "time without time zone",
    "Recurring local calendar dates and times retain their types");
Check(Entity(typeof(NguoiDung)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "EmailChuanHoa"), "Normalized email unique");
Check(Entity(typeof(NguoiDung)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "MaSoSinhVien" && x.GetFilter() == "\"ma_so_sinh_vien\" IS NOT NULL"),
    "Student code optional but unique when present");
foreach (var name in new[] { "TenTruong", "TenLop" })
{
    var property = Entity(typeof(NguoiDung)).FindProperty(name);
    Check(property != null && property.IsNullable && property.GetMaxLength() == 200,
        "Self-declared profile field " + name + " is optional and limited to 200 characters");
}
Check(Entity(typeof(ThongBao)).GetIndexes().Any(x => x.IsUnique &&
    Names(x) == "MaNhiemVu,Loai"), "One reminder per task and type");
Check(Entity(typeof(ThongBao)).GetIndexes().Count(x =>
    Names(x) == "MaNguoiDung,NgayPhat" && x.GetFilter() != null) == 2,
    "Delivered and unread inbox indexes both preserved");
var priority = Entity(typeof(NhiemVu)).FindProperty("MucDoUuTien")!;
var reminderHours = Entity(typeof(NguoiDung)).FindProperty("NhacTruocGio")!;
Check(Equals(reminderHours.Sentinel, 24) && !Equals(reminderHours.Sentinel, 0),
    "Explicit zero reminder hours is not replaced by DB default");
Check(Equals(priority.Sentinel, MucDoUuTienNhiemVu.Medium) &&
      !Equals(priority.Sentinel, MucDoUuTienNhiemVu.Low), "Explicit Low priority is not replaced by DB default");
foreach (var entity in entities)
{
    foreach (var property in entity.GetProperties().Where(x => x.ClrType.IsEnum))
        Check(property.GetTypeMapping().Converter!.ProviderClrType == typeof(string) &&
              property.GetMaxLength() == 32, entity.ClrType.Name + "." + property.Name + " string enum");
}

// Compile PostgreSQL DDL without a connection; real FK/CHECK enforcement needs PostgreSQL tests.
var sql = context.Database.GenerateCreateScript();
Check(expectedTables.All(table => sql.Contains("CREATE TABLE " + table + " (")),
    "PostgreSQL SQL generated for all tables");
Check(sql.Contains("CK_task_items_state") && sql.Contains("CK_notifications_read") &&
      sql.Contains("CK_class_sessions_times"), "Critical CHECK constraints included in SQL");
Check(sql.Contains("ON DELETE SET NULL") && sql.Contains("ON DELETE RESTRICT") &&
      sql.Contains("ON DELETE CASCADE"), "Delete behaviors included in PostgreSQL SQL");
Console.WriteLine("All schema checks passed. No database connection was opened.");

IEntityType Entity(Type type) => model.FindEntityType(type)!;
static string Names(IIndex index) => string.Join(",", index.Properties.Select(x => x.Name));
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    Console.WriteLine("PASS: " + message);
}
