using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DonBangToChucVaLichNhacCu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Khóa trong cùng giao dịch để không mất liên kết/hàng đợi vừa phát sinh lúc chuyển đổi.
            migrationBuilder.Sql("""
                LOCK TABLE nguoi_dung, lop_sinh_vien, khoa, khoa_hoc, truong, lich_nhac, thong_bao IN ACCESS EXCLUSIVE MODE;
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM lich_nhac WHERE trang_thai IN ('Pending','Failed'))
                       OR EXISTS (SELECT 1 FROM lich_nhac l WHERE l.trang_thai='Delivered'
                          AND NOT EXISTS (SELECT 1 FROM thong_bao t WHERE t.ma_lich_nhac=l.ma
                              AND t.ma_nguoi_dung=l.ma_nguoi_dung AND t.ma_nhiem_vu=l.ma_nhiem_vu AND t.trang_thai_phat='Delivered'))
                    THEN RAISE EXCEPTION 'LEGACY_REMINDERS_REQUIRE_REVIEW: Cần xử lý hàng đợi cũ trước khi gỡ bảng.';
                    END IF;
                END $$;
                UPDATE nguoi_dung n SET
                    ten_truong=COALESCE(NULLIF(btrim(n.ten_truong),''), t.ten),
                    ten_lop=COALESCE(NULLIF(btrim(n.ten_lop),''), l.ten)
                FROM lop_sinh_vien l JOIN khoa k ON k.ma=l.ma_khoa JOIN truong t ON t.ma=k.ma_truong
                WHERE n.ma_lop=l.ma;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_users_student_classes_ClassId",
                table: "nguoi_dung");

            migrationBuilder.DropForeignKey(
                name: "FK_thong_bao_lich_nhac_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao");

            migrationBuilder.DropTable(
                name: "lich_nhac");

            migrationBuilder.DropTable(
                name: "lop_sinh_vien");

            migrationBuilder.DropTable(
                name: "khoa_hoc");

            migrationBuilder.DropTable(
                name: "khoa");

            migrationBuilder.DropTable(
                name: "truong");

            migrationBuilder.DropIndex(
                name: "IX_thong_bao_ma_lich_nhac",
                table: "thong_bao");

            migrationBuilder.DropIndex(
                name: "IX_thong_bao_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao");

            migrationBuilder.DropIndex(
                name: "IX_users_ClassId",
                table: "nguoi_dung");

            migrationBuilder.DropColumn(
                name: "ma_lich_nhac",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "ma_lop",
                table: "nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new InvalidOperationException("Không tự khôi phục bảng rỗng. Cần dùng bản sao lưu để phục hồi dữ liệu tổ chức và hàng đợi cũ.");
        }
    }
}
