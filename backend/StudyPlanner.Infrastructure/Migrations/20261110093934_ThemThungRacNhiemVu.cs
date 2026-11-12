using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ThemThungRacNhiemVu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_task_histories_action_values",
                table: "lich_su_nhiem_vu");

            migrationBuilder.AddColumn<int>(
                name: "ma_cha_truoc_khi_xoa",
                table: "nhiem_vu",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_nhom_xoa",
                table: "nhiem_vu",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ngay_xoa",
                table: "nhiem_vu",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_nhiem_vu_ma_nguoi_dung_ma_nhom_xoa",
                table: "nhiem_vu",
                columns: new[] { "ma_nguoi_dung", "ma_nhom_xoa" },
                filter: "ngay_xoa IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_task_histories_action_values",
                table: "lich_su_nhiem_vu",
                sql: "\"hanh_dong\" IN ('Created', 'Updated', 'Completed', 'Reopened', 'Deleted', 'Trashed', 'Restored')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM nhiem_vu WHERE ngay_xoa IS NOT NULL) THEN
                        RAISE EXCEPTION 'Restore or purge archived tasks before removing the task trash migration.';
                    END IF;
                END;
                $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_nhiem_vu_ma_nguoi_dung_ma_nhom_xoa",
                table: "nhiem_vu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_task_histories_action_values",
                table: "lich_su_nhiem_vu");

            migrationBuilder.Sql("UPDATE lich_su_nhiem_vu SET hanh_dong = 'Updated' WHERE hanh_dong IN ('Trashed', 'Restored');");

            migrationBuilder.DropColumn(
                name: "ma_cha_truoc_khi_xoa",
                table: "nhiem_vu");

            migrationBuilder.DropColumn(
                name: "ma_nhom_xoa",
                table: "nhiem_vu");

            migrationBuilder.DropColumn(
                name: "ngay_xoa",
                table: "nhiem_vu");

            migrationBuilder.AddCheckConstraint(
                name: "CK_task_histories_action_values",
                table: "lich_su_nhiem_vu",
                sql: "\"hanh_dong\" IN ('Created', 'Updated', 'Completed', 'Reopened', 'Deleted')");
        }
    }
}
