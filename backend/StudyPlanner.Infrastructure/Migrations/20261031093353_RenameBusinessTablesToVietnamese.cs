using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameBusinessTablesToVietnamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                newName: "nguoi_dung");

            migrationBuilder.RenameTable(
                name: "task_items",
                newName: "nhiem_vu");

            migrationBuilder.RenameTable(
                name: "task_histories",
                newName: "lich_su_nhiem_vu");

            migrationBuilder.RenameTable(
                name: "student_classes",
                newName: "lop_sinh_vien");

            migrationBuilder.RenameTable(
                name: "semesters",
                newName: "hoc_ky");

            migrationBuilder.RenameTable(
                name: "schools",
                newName: "truong");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "thong_bao");

            migrationBuilder.RenameTable(
                name: "faculties",
                newName: "khoa");

            migrationBuilder.RenameTable(
                name: "courses",
                newName: "mon_hoc");

            migrationBuilder.RenameTable(
                name: "cohorts",
                newName: "khoa_hoc");

            migrationBuilder.RenameTable(
                name: "class_sessions",
                newName: "lich_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "truong",
                newName: "schools");

            migrationBuilder.RenameTable(
                name: "thong_bao",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "nhiem_vu",
                newName: "task_items");

            migrationBuilder.RenameTable(
                name: "nguoi_dung",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "mon_hoc",
                newName: "courses");

            migrationBuilder.RenameTable(
                name: "lop_sinh_vien",
                newName: "student_classes");

            migrationBuilder.RenameTable(
                name: "lich_su_nhiem_vu",
                newName: "task_histories");

            migrationBuilder.RenameTable(
                name: "lich_hoc",
                newName: "class_sessions");

            migrationBuilder.RenameTable(
                name: "khoa_hoc",
                newName: "cohorts");

            migrationBuilder.RenameTable(
                name: "khoa",
                newName: "faculties");

            migrationBuilder.RenameTable(
                name: "hoc_ky",
                newName: "semesters");
        }
    }
}
