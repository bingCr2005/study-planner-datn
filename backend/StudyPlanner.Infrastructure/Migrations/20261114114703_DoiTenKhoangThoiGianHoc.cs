using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations;

/// <summary>Đổi tên khoảng thời gian học và các ràng buộc tại chỗ để giữ nguyên dữ liệu.</summary>
public partial class DoiTenKhoangThoiGianHoc : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => DoiTen(migrationBuilder, false);
    protected override void Down(MigrationBuilder migrationBuilder) => DoiTen(migrationBuilder, true);

    private static void DoiTen(MigrationBuilder builder, bool hoanTac)
    {
        var bangCu = hoanTac ? "khoang_thoi_gian_hoc" : "doan_thuc_hoc";
        var bangMoi = hoanTac ? "doan_thuc_hoc" : "khoang_thoi_gian_hoc";
        // Chỉ đổi tên đối tượng; giữ nguyên mã bản ghi, dữ liệu, liên kết và bộ đếm tự tăng.
        builder.RenameTable(name: bangCu, newName: bangMoi);
        (string Cu, string Moi)[] rangBuoc = [
            ("PK_doan_thuc_hoc", "PK_khoang_thoi_gian_hoc"),
            ("CK_doan_thuc_hoc_times", "CK_khoang_thoi_gian_hoc_times"),
            ("FK_doan_thuc_hoc_nguoi_dung_ma_nguoi_dung", "FK_khoang_thoi_gian_hoc_nguoi_dung_ma_nguoi_dung"),
            ("FK_doan_thuc_hoc_nhiem_vu_ma_nhiem_vu_ma_nguoi_dung", "FK_khoang_thoi_gian_hoc_nhiem_vu_ma_nhiem_vu_ma_nguoi_dung"),
            ("FK_doan_thuc_hoc_phien_tap_trung_ma_phien_tap_trung_ma_nguoi_d~", "FK_khoang_thoi_gian_hoc_phien_tap_trung_ma_phien_tap_trung_ma_~"),
            ("FK_doan_thuc_hoc_phien_tu_hoc_ma_phien_tu_hoc_ma_nhiem_vu_ma_n~", "FK_khoang_thoi_gian_hoc_phien_tu_hoc_ma_phien_tu_hoc_ma_nhiem_~")
        ];
        foreach (var (cu, moi) in rangBuoc)
            builder.Sql($"ALTER TABLE \"{bangMoi}\" RENAME CONSTRAINT \"{(hoanTac ? moi : cu)}\" TO \"{(hoanTac ? cu : moi)}\";");
        (string Cu, string Moi)[] chiMuc = [
            ("IX_doan_thuc_hoc_ma_nguoi_dung", "IX_khoang_thoi_gian_hoc_ma_nguoi_dung"),
            ("IX_doan_thuc_hoc_ma_nhiem_vu_ma_nguoi_dung", "IX_khoang_thoi_gian_hoc_ma_nhiem_vu_ma_nguoi_dung"),
            ("IX_doan_thuc_hoc_ma_phien_tap_trung_ma_nguoi_dung", "IX_khoang_thoi_gian_hoc_ma_phien_tap_trung_ma_nguoi_dung"),
            ("IX_doan_thuc_hoc_ma_phien_tu_hoc_ma_nhiem_vu_ma_nguoi_dung", "IX_khoang_thoi_gian_hoc_ma_phien_tu_hoc_ma_nhiem_vu_ma_nguoi_d~")
        ];
        foreach (var (cu, moi) in chiMuc)
            builder.RenameIndex(name: hoanTac ? moi : cu, table: bangMoi, newName: hoanTac ? cu : moi);
        // PostgreSQL không tự đổi tên sequence của cột identity khi đổi tên bảng.
        builder.Sql($"ALTER SEQUENCE \"{bangCu}_ma_seq\" RENAME TO \"{bangMoi}_ma_seq\";");
    }
}
