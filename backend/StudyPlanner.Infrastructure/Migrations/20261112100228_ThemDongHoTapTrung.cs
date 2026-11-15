using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ThemDongHoTapTrung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_phien_tap_trung",
                table: "doan_thuc_hoc",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "phien_tap_trung",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_nhiem_vu = table.Column<int>(type: "integer", nullable: true),
                    ma_phien_tu_hoc = table.Column<int>(type: "integer", nullable: true),
                    tieu_de = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    che_do = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    trang_thai = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    muc_tieu_giay = table.Column<int>(type: "integer", nullable: false),
                    so_mili_giay_da_hoc = table.Column<long>(type: "bigint", nullable: false),
                    bat_dau = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    bat_dau_doan = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ket_thuc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ghi_chu = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    nhap_thu_cong = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phien_tap_trung", x => x.ma);
                    table.UniqueConstraint("AK_phien_tap_trung_ma_ma_nguoi_dung", x => new { x.ma, x.ma_nguoi_dung });
                    table.CheckConstraint("CK_focus_mode", "che_do IN ('Pomodoro', 'Stopwatch') AND muc_tieu_giay BETWEEN 60 AND 10800");
                    table.CheckConstraint("CK_focus_state", "trang_thai IN ('Running', 'Paused', 'Completed', 'Discarded') AND so_mili_giay_da_hoc >= 0 AND ((trang_thai = 'Running' AND bat_dau_doan IS NOT NULL AND ket_thuc IS NULL) OR (trang_thai = 'Paused' AND bat_dau_doan IS NULL AND ket_thuc IS NULL) OR (trang_thai IN ('Completed', 'Discarded') AND bat_dau_doan IS NULL AND ket_thuc IS NOT NULL))");
                    table.CheckConstraint("CK_focus_times", "(ket_thuc IS NULL OR ket_thuc >= bat_dau) AND (bat_dau_doan IS NULL OR bat_dau_doan >= bat_dau)");
                    table.ForeignKey(
                        name: "FK_phien_tap_trung_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_phien_tap_trung_nhiem_vu_ma_nhiem_vu",
                        column: x => x.ma_nhiem_vu,
                        principalTable: "nhiem_vu",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_phien_tap_trung_phien_tu_hoc_ma_phien_tu_hoc",
                        column: x => x.ma_phien_tu_hoc,
                        principalTable: "phien_tu_hoc",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_doan_thuc_hoc_ma_phien_tap_trung_ma_nguoi_dung",
                table: "doan_thuc_hoc",
                columns: new[] { "ma_phien_tap_trung", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_phien_tap_trung_ma_nguoi_dung",
                table: "phien_tap_trung",
                column: "ma_nguoi_dung",
                unique: true,
                filter: "trang_thai IN ('Running', 'Paused')");

            migrationBuilder.CreateIndex(
                name: "IX_phien_tap_trung_ma_nguoi_dung_bat_dau",
                table: "phien_tap_trung",
                columns: new[] { "ma_nguoi_dung", "bat_dau" });

            migrationBuilder.CreateIndex(
                name: "IX_phien_tap_trung_ma_nguoi_dung_ma_phien_tu_hoc",
                table: "phien_tap_trung",
                columns: new[] { "ma_nguoi_dung", "ma_phien_tu_hoc" },
                unique: true,
                filter: "ma_phien_tu_hoc IS NOT NULL AND trang_thai <> 'Discarded'");

            migrationBuilder.CreateIndex(
                name: "IX_phien_tap_trung_ma_nhiem_vu",
                table: "phien_tap_trung",
                column: "ma_nhiem_vu");

            migrationBuilder.CreateIndex(
                name: "IX_phien_tap_trung_ma_phien_tu_hoc",
                table: "phien_tap_trung",
                column: "ma_phien_tu_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_doan_thuc_hoc_phien_tap_trung_ma_phien_tap_trung_ma_nguoi_d~",
                table: "doan_thuc_hoc",
                columns: new[] { "ma_phien_tap_trung", "ma_nguoi_dung" },
                principalTable: "phien_tap_trung",
                principalColumns: new[] { "ma", "ma_nguoi_dung" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doan_thuc_hoc_phien_tap_trung_ma_phien_tap_trung_ma_nguoi_d~",
                table: "doan_thuc_hoc");

            migrationBuilder.DropTable(
                name: "phien_tap_trung");

            migrationBuilder.DropIndex(
                name: "IX_doan_thuc_hoc_ma_phien_tap_trung_ma_nguoi_dung",
                table: "doan_thuc_hoc");

            migrationBuilder.DropColumn(
                name: "ma_phien_tap_trung",
                table: "doan_thuc_hoc");
        }
    }
}
