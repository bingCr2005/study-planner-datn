using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tieu_de",
                table: "thong_bao",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Nhắc hạn nhiệm vụ");

            migrationBuilder.AddCheckConstraint(
                name: "CK_thong_bao_tieu_de",
                table: "thong_bao",
                sql: "length(btrim(tieu_de)) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_thong_bao_tieu_de",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "tieu_de",
                table: "thong_bao");
        }
    }
}
