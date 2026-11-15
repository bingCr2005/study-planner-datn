using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CaiTienLapKeHoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "gioi_han_hoc_moi_ngay_phut",
                table: "nguoi_dung",
                type: "integer",
                nullable: false,
                defaultValue: 180);

            migrationBuilder.AddColumn<int>(
                name: "nghi_giua_phien_phut",
                table: "nguoi_dung",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "phien_hoc_phut",
                table: "nguoi_dung",
                type: "integer",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_khung_gio_ranh_ma_ma_nguoi_dung",
                table: "khung_gio_ranh",
                columns: new[] { "ma", "ma_nguoi_dung" });

            migrationBuilder.CreateTable(
                name: "cau_hinh_nhac_ke_hoach",
                columns: table => new
                {
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    dang_bat = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_cap_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cau_hinh_nhac_ke_hoach", x => x.ma_nguoi_dung);
                    table.ForeignKey(
                        name: "FK_cau_hinh_nhac_ke_hoach_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ngoai_le_gio_ranh",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_khung_gio_ranh = table.Column<int>(type: "integer", nullable: false),
                    ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    bo_qua = table.Column<bool>(type: "boolean", nullable: false),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_cap_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ngoai_le_gio_ranh", x => x.ma);
                    table.CheckConstraint("CK_ngoai_le_gio_ranh_times", "(bo_qua AND gio_bat_dau IS NULL AND gio_ket_thuc IS NULL) OR (NOT bo_qua AND gio_bat_dau IS NOT NULL AND gio_ket_thuc IS NOT NULL AND gio_ket_thuc > gio_bat_dau)");
                    table.ForeignKey(
                        name: "FK_ngoai_le_gio_ranh_khung_gio_ranh_ma_khung_gio_ranh_ma_nguoi~",
                        columns: x => new { x.ma_khung_gio_ranh, x.ma_nguoi_dung },
                        principalTable: "khung_gio_ranh",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nhac_ke_hoach",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    khoa_chong_trung = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    loai = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tieu_de = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    noi_dung = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    duong_dan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ngay_phat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_doc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhac_ke_hoach", x => x.ma);
                    table.CheckConstraint("CK_nhac_ke_hoach_doc", "ngay_doc IS NULL OR ngay_doc >= ngay_phat");
                    table.CheckConstraint("CK_nhac_ke_hoach_duong_dan", "duong_dan IN ('/tasks?tab=planning', '/tasks?tab=free')");
                    table.CheckConstraint("CK_nhac_ke_hoach_loai", "loai IN ('WeeklyReview', 'AvailabilityExpiring')");
                    table.CheckConstraint("CK_nhac_ke_hoach_noi_dung", "length(btrim(tieu_de)) > 0 AND length(btrim(noi_dung)) > 0 AND length(btrim(khoa_chong_trung)) > 0");
                    table.ForeignKey(
                        name: "FK_nhac_ke_hoach_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_nguoi_dung_cau_hinh_hoc",
                table: "nguoi_dung",
                sql: "phien_hoc_phut BETWEEN 15 AND 50 AND nghi_giua_phien_phut BETWEEN 1 AND 30 AND gioi_han_hoc_moi_ngay_phut BETWEEN 15 AND 720");

            migrationBuilder.CreateIndex(
                name: "IX_ngoai_le_gio_ranh_ma_khung_gio_ranh_ma_nguoi_dung",
                table: "ngoai_le_gio_ranh",
                columns: new[] { "ma_khung_gio_ranh", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_ngoai_le_gio_ranh_ma_khung_gio_ranh_ngay",
                table: "ngoai_le_gio_ranh",
                columns: new[] { "ma_khung_gio_ranh", "ngay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ngoai_le_gio_ranh_ma_nguoi_dung_ngay",
                table: "ngoai_le_gio_ranh",
                columns: new[] { "ma_nguoi_dung", "ngay" });

            migrationBuilder.CreateIndex(
                name: "IX_nhac_ke_hoach_ma_nguoi_dung_khoa_chong_trung",
                table: "nhac_ke_hoach",
                columns: new[] { "ma_nguoi_dung", "khoa_chong_trung" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nhac_ke_hoach_ma_nguoi_dung_ngay_phat",
                table: "nhac_ke_hoach",
                columns: new[] { "ma_nguoi_dung", "ngay_phat" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cau_hinh_nhac_ke_hoach");

            migrationBuilder.DropTable(
                name: "ngoai_le_gio_ranh");

            migrationBuilder.DropTable(
                name: "nhac_ke_hoach");

            migrationBuilder.DropCheckConstraint(
                name: "CK_nguoi_dung_cau_hinh_hoc",
                table: "nguoi_dung");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_khung_gio_ranh_ma_ma_nguoi_dung",
                table: "khung_gio_ranh");

            migrationBuilder.DropColumn(
                name: "gioi_han_hoc_moi_ngay_phut",
                table: "nguoi_dung");

            migrationBuilder.DropColumn(
                name: "nghi_giua_phien_phut",
                table: "nguoi_dung");

            migrationBuilder.DropColumn(
                name: "phien_hoc_phut",
                table: "nguoi_dung");
        }
    }
}
