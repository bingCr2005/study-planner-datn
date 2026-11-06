using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GioiHanMotPhienDangHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_phien_tu_hoc_dang_hoc",
                table: "phien_tu_hoc",
                column: "ma_nguoi_dung",
                unique: true,
                filter: "trang_thai = 'InProgress'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_phien_tu_hoc_dang_hoc",
                table: "phien_tu_hoc");
        }
    }
}
