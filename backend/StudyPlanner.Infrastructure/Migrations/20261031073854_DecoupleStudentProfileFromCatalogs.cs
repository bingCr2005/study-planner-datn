using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleStudentProfileFromCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ten_lop",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ten_truong",
                table: "users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // Chụp tên hiển thị từ liên kết cũ; giữ nguyên ma_lop và toàn bộ bảng danh mục.
            // Không ghi đè hồ sơ tự khai nếu bước chuyển dữ liệu được tái sử dụng sau này.
            migrationBuilder.Sql("""
                UPDATE users AS u
                SET ten_lop = COALESCE(u.ten_lop,
                        CASE WHEN c.ten ~ '[^[:space:]]' THEN btrim(c.ten) END),
                    ten_truong = COALESCE(u.ten_truong,
                        CASE WHEN s.ten ~ '[^[:space:]]' THEN btrim(s.ten) END)
                FROM student_classes AS c
                JOIN faculties AS f ON f.ma = c.ma_khoa
                JOIN schools AS s ON s.ma = f.ma_truong
                WHERE u.ma_lop = c.ma;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_ten_lop_not_blank",
                table: "users",
                sql: "\"ten_lop\" IS NULL OR \"ten_lop\" ~ '[^[:space:]]'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_users_ten_truong_not_blank",
                table: "users",
                sql: "\"ten_truong\" IS NULL OR \"ten_truong\" ~ '[^[:space:]]'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_users_ten_lop_not_blank",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_users_ten_truong_not_blank",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ten_lop",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ten_truong",
                table: "users");
        }
    }
}
