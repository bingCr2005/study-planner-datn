using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPlanningModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_task_items_courses_CourseId",
                table: "nhiem_vu");

            migrationBuilder.AddColumn<int>(
                name: "ma_lich_nhac",
                table: "thong_bao",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "bat_dau_su_kien",
                table: "nhiem_vu",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ket_thuc_su_kien",
                table: "nhiem_vu",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_hoc_ky",
                table: "nhiem_vu",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "phien_ban_lap_lich",
                table: "nguoi_dung",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "nguon_thay_doi",
                table: "lich_su_nhiem_vu",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_ap_dung_den",
                table: "lich_hoc",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_ap_dung_tu",
                table: "lich_hoc",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "da_luu_tru",
                table: "hoc_ky",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Dừng khi dữ liệu cũ khác chủ, không tự đổi người sở hữu để vượt FK mới.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM nhiem_vu t JOIN mon_hoc c ON c.ma=t.ma_mon_hoc
                        JOIN hoc_ky h ON h.ma=c.ma_hoc_ky WHERE t.ma_nguoi_dung<>h.ma_nguoi_dung)
                    THEN RAISE EXCEPTION 'Task/course ownership mismatch; resolve before migration'; END IF;
                END $$;
                UPDATE nhiem_vu t SET ma_hoc_ky=c.ma_hoc_ky FROM mon_hoc c WHERE c.ma=t.ma_mon_hoc;
                """);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_nhiem_vu_ma_ma_nguoi_dung",
                table: "nhiem_vu",
                columns: new[] { "ma", "ma_nguoi_dung" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_mon_hoc_ma_ma_hoc_ky",
                table: "mon_hoc",
                columns: new[] { "ma", "ma_hoc_ky" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_hoc_ky_ma_ma_nguoi_dung",
                table: "hoc_ky",
                columns: new[] { "ma", "ma_nguoi_dung" });

            migrationBuilder.CreateTable(
                name: "khung_gio_ranh",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    thu_trong_tuan = table.Column<int>(type: "integer", nullable: false),
                    gio_bat_dau = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    gio_ket_thuc = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: false),
                    dang_su_dung = table.Column<bool>(type: "boolean", nullable: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_cap_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_khung_gio_ranh", x => x.ma);
                    table.CheckConstraint("CK_khung_gio_ranh_times", "thu_trong_tuan BETWEEN 1 AND 7 AND gio_ket_thuc > gio_bat_dau AND ngay_ket_thuc >= ngay_bat_dau");
                    table.ForeignKey(
                        name: "FK_khung_gio_ranh_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lan_ap_dung_lich",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    khoa_yeu_cau = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    dau_kiem_noi_dung = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    phien_ban_nguon = table.Column<long>(type: "bigint", nullable: false),
                    phien_ban_quy_tac = table.Column<int>(type: "integer", nullable: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lan_ap_dung_lich", x => x.ma);
                    table.UniqueConstraint("AK_lan_ap_dung_lich_ma_ma_nguoi_dung", x => new { x.ma, x.ma_nguoi_dung });
                    table.CheckConstraint("CK_lan_ap_dung_lich_key", "length(btrim(khoa_yeu_cau)) > 0 AND dau_kiem_noi_dung ~ '^[0-9a-fA-F]{64}$' AND phien_ban_nguon >= 0 AND phien_ban_quy_tac > 0");
                    table.ForeignKey(
                        name: "FK_lan_ap_dung_lich_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lich_nhac",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_nhiem_vu = table.Column<int>(type: "integer", nullable: false),
                    nhac_truoc_phut = table.Column<int>(type: "integer", nullable: true),
                    moc_nhac = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    thoi_diem_nhac = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trang_thai = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    so_lan_thu = table.Column<int>(type: "integer", nullable: false),
                    lan_thu_tiep_theo_luc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ngay_phat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ma_thong_bao_cu = table.Column<int>(type: "integer", nullable: true),
                    noi_dung = table.Column<string>(type: "text", nullable: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_cap_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_nhac", x => x.ma);
                    table.UniqueConstraint("AK_lich_nhac_ma_ma_nguoi_dung_ma_nhiem_vu", x => new { x.ma, x.ma_nguoi_dung, x.ma_nhiem_vu });
                    table.CheckConstraint("CK_lich_nhac_status", "trang_thai IN ('Pending','Delivered','Cancelled','Failed') AND ((trang_thai = 'Delivered' AND ngay_phat IS NOT NULL) OR (trang_thai <> 'Delivered' AND ngay_phat IS NULL))");
                    table.CheckConstraint("CK_lich_nhac_values", "(nhac_truoc_phut IS NULL OR nhac_truoc_phut >= 0) AND so_lan_thu >= 0 AND length(btrim(moc_nhac)) > 0 AND length(btrim(noi_dung)) > 0");
                    table.ForeignKey(
                        name: "FK_lich_nhac_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma");
                    table.ForeignKey(
                        name: "FK_lich_nhac_nhiem_vu_ma_nhiem_vu_ma_nguoi_dung",
                        columns: x => new { x.ma_nhiem_vu, x.ma_nguoi_dung },
                        principalTable: "nhiem_vu",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quy_tac_uu_tien",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ten = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phien_ban = table.Column<int>(type: "integer", nullable: false),
                    dang_su_dung = table.Column<bool>(type: "boolean", nullable: false),
                    cau_hinh = table.Column<string>(type: "jsonb", nullable: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quy_tac_uu_tien", x => x.ma);
                    table.CheckConstraint("CK_quy_tac_uu_tien_values", "phien_ban > 0 AND length(btrim(ten)) > 0 AND jsonb_typeof(cau_hinh) = 'object'");
                });

            migrationBuilder.CreateTable(
                name: "tep_dinh_kem_nhiem_vu",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nhiem_vu = table.Column<int>(type: "integer", nullable: false),
                    ma_nguoi_tai_len = table.Column<int>(type: "integer", nullable: false),
                    ten_tep = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    khoa_luu_tru = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    loai_tep = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: false),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tep_dinh_kem_nhiem_vu", x => x.ma);
                    table.CheckConstraint("CK_tep_dinh_kem_nhiem_vu_values", "kich_thuoc_byte > 0 AND length(btrim(ten_tep)) > 0 AND length(btrim(khoa_luu_tru)) > 0 AND length(btrim(loai_tep)) > 0");
                    table.ForeignKey(
                        name: "FK_tep_dinh_kem_nhiem_vu_nguoi_dung_ma_nguoi_tai_len",
                        column: x => x.ma_nguoi_tai_len,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma");
                    table.ForeignKey(
                        name: "FK_tep_dinh_kem_nhiem_vu_nhiem_vu_ma_nhiem_vu_ma_nguoi_tai_len",
                        columns: x => new { x.ma_nhiem_vu, x.ma_nguoi_tai_len },
                        principalTable: "nhiem_vu",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "phien_tu_hoc",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_nhiem_vu = table.Column<int>(type: "integer", nullable: false),
                    ma_lan_ap_dung = table.Column<int>(type: "integer", nullable: true),
                    bat_dau_du_kien = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ket_thuc_du_kien = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trang_thai = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ghi_chu = table.Column<string>(type: "text", nullable: true),
                    ngay_tao = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ngay_cap_nhat = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phien_tu_hoc", x => x.ma);
                    table.UniqueConstraint("AK_phien_tu_hoc_ma_ma_nhiem_vu_ma_nguoi_dung", x => new { x.ma, x.ma_nhiem_vu, x.ma_nguoi_dung });
                    table.CheckConstraint("CK_phien_tu_hoc_status", "trang_thai IN ('Planned','InProgress','Completed','Missed','Cancelled')");
                    table.CheckConstraint("CK_phien_tu_hoc_times", "ket_thuc_du_kien > bat_dau_du_kien");
                    table.ForeignKey(
                        name: "FK_phien_tu_hoc_lan_ap_dung_lich_ma_lan_ap_dung_ma_nguoi_dung",
                        columns: x => new { x.ma_lan_ap_dung, x.ma_nguoi_dung },
                        principalTable: "lan_ap_dung_lich",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" });
                    table.ForeignKey(
                        name: "FK_phien_tu_hoc_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma");
                    table.ForeignKey(
                        name: "FK_phien_tu_hoc_nhiem_vu_ma_nhiem_vu_ma_nguoi_dung",
                        columns: x => new { x.ma_nhiem_vu, x.ma_nguoi_dung },
                        principalTable: "nhiem_vu",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doan_thuc_hoc",
                columns: table => new
                {
                    ma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ma_nguoi_dung = table.Column<int>(type: "integer", nullable: false),
                    ma_nhiem_vu = table.Column<int>(type: "integer", nullable: false),
                    ma_phien_tu_hoc = table.Column<int>(type: "integer", nullable: true),
                    bat_dau_thuc_te = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ket_thuc_thuc_te = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doan_thuc_hoc", x => x.ma);
                    table.CheckConstraint("CK_doan_thuc_hoc_times", "ket_thuc_thuc_te IS NULL OR ket_thuc_thuc_te > bat_dau_thuc_te");
                    table.ForeignKey(
                        name: "FK_doan_thuc_hoc_nguoi_dung_ma_nguoi_dung",
                        column: x => x.ma_nguoi_dung,
                        principalTable: "nguoi_dung",
                        principalColumn: "ma");
                    table.ForeignKey(
                        name: "FK_doan_thuc_hoc_nhiem_vu_ma_nhiem_vu_ma_nguoi_dung",
                        columns: x => new { x.ma_nhiem_vu, x.ma_nguoi_dung },
                        principalTable: "nhiem_vu",
                        principalColumns: new[] { "ma", "ma_nguoi_dung" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doan_thuc_hoc_phien_tu_hoc_ma_phien_tu_hoc_ma_nhiem_vu_ma_n~",
                        columns: x => new { x.ma_phien_tu_hoc, x.ma_nhiem_vu, x.ma_nguoi_dung },
                        principalTable: "phien_tu_hoc",
                        principalColumns: new[] { "ma", "ma_nhiem_vu", "ma_nguoi_dung" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_ma_lich_nhac",
                table: "thong_bao",
                column: "ma_lich_nhac",
                unique: true,
                filter: "ma_lich_nhac IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao",
                columns: new[] { "ma_lich_nhac", "ma_nguoi_dung", "ma_nhiem_vu" });

            migrationBuilder.CreateIndex(
                name: "IX_nhiem_vu_ma_hoc_ky_ma_nguoi_dung",
                table: "nhiem_vu",
                columns: new[] { "ma_hoc_ky", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_nhiem_vu_ma_mon_hoc_ma_hoc_ky",
                table: "nhiem_vu",
                columns: new[] { "ma_mon_hoc", "ma_hoc_ky" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_nhiem_vu_mon_hoc_ky",
                table: "nhiem_vu",
                sql: "ma_mon_hoc IS NULL OR ma_hoc_ky IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_nhiem_vu_su_kien",
                table: "nhiem_vu",
                sql: "(bat_dau_su_kien IS NULL AND ket_thuc_su_kien IS NULL) OR (bat_dau_su_kien IS NOT NULL AND ket_thuc_su_kien IS NOT NULL AND ket_thuc_su_kien > bat_dau_su_kien)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_nguoi_dung_revision",
                table: "nguoi_dung",
                sql: "phien_ban_lap_lich >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_lich_su_nguon",
                table: "lich_su_nhiem_vu",
                sql: "nguon_thay_doi IN ('User','System')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_lich_hoc_ngay_ap_dung",
                table: "lich_hoc",
                sql: "(ngay_ap_dung_tu IS NULL AND ngay_ap_dung_den IS NULL) OR (ngay_ap_dung_tu IS NOT NULL AND ngay_ap_dung_den IS NOT NULL AND ngay_ap_dung_den >= ngay_ap_dung_tu)");

            migrationBuilder.CreateIndex(
                name: "IX_doan_thuc_hoc_ma_nguoi_dung",
                table: "doan_thuc_hoc",
                column: "ma_nguoi_dung",
                unique: true,
                filter: "ket_thuc_thuc_te IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_doan_thuc_hoc_ma_nhiem_vu_ma_nguoi_dung",
                table: "doan_thuc_hoc",
                columns: new[] { "ma_nhiem_vu", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_doan_thuc_hoc_ma_phien_tu_hoc_ma_nhiem_vu_ma_nguoi_dung",
                table: "doan_thuc_hoc",
                columns: new[] { "ma_phien_tu_hoc", "ma_nhiem_vu", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_khung_gio_ranh_ma_nguoi_dung_thu_trong_tuan_gio_bat_dau",
                table: "khung_gio_ranh",
                columns: new[] { "ma_nguoi_dung", "thu_trong_tuan", "gio_bat_dau" });

            migrationBuilder.CreateIndex(
                name: "IX_lan_ap_dung_lich_ma_nguoi_dung_khoa_yeu_cau",
                table: "lan_ap_dung_lich",
                columns: new[] { "ma_nguoi_dung", "khoa_yeu_cau" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lich_nhac_lan_thu_tiep_theo_luc",
                table: "lich_nhac",
                column: "lan_thu_tiep_theo_luc",
                filter: "trang_thai = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_lich_nhac_ma_nguoi_dung",
                table: "lich_nhac",
                column: "ma_nguoi_dung");

            migrationBuilder.CreateIndex(
                name: "IX_lich_nhac_ma_nhiem_vu_ma_nguoi_dung",
                table: "lich_nhac",
                columns: new[] { "ma_nhiem_vu", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_nhac_ma_thong_bao_cu",
                table: "lich_nhac",
                column: "ma_thong_bao_cu",
                unique: true,
                filter: "ma_thong_bao_cu IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_lich_nhac_thoi_diem_nhac",
                table: "lich_nhac",
                column: "thoi_diem_nhac",
                filter: "trang_thai = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_phien_tu_hoc_ma_lan_ap_dung_ma_nguoi_dung",
                table: "phien_tu_hoc",
                columns: new[] { "ma_lan_ap_dung", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_phien_tu_hoc_ma_nguoi_dung_bat_dau_du_kien",
                table: "phien_tu_hoc",
                columns: new[] { "ma_nguoi_dung", "bat_dau_du_kien" });

            migrationBuilder.CreateIndex(
                name: "IX_phien_tu_hoc_ma_nhiem_vu_ma_nguoi_dung",
                table: "phien_tu_hoc",
                columns: new[] { "ma_nhiem_vu", "ma_nguoi_dung" });

            migrationBuilder.CreateIndex(
                name: "IX_quy_tac_uu_tien_dang_su_dung",
                table: "quy_tac_uu_tien",
                column: "dang_su_dung",
                unique: true,
                filter: "dang_su_dung = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_quy_tac_uu_tien_phien_ban",
                table: "quy_tac_uu_tien",
                column: "phien_ban",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tep_dinh_kem_nhiem_vu_khoa_luu_tru",
                table: "tep_dinh_kem_nhiem_vu",
                column: "khoa_luu_tru",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tep_dinh_kem_nhiem_vu_ma_nguoi_tai_len",
                table: "tep_dinh_kem_nhiem_vu",
                column: "ma_nguoi_tai_len");

            migrationBuilder.CreateIndex(
                name: "IX_tep_dinh_kem_nhiem_vu_ma_nhiem_vu_ma_nguoi_tai_len",
                table: "tep_dinh_kem_nhiem_vu",
                columns: new[] { "ma_nhiem_vu", "ma_nguoi_tai_len" });

            // Expand/backfill: giữ nguyên thong_bao cũ để code đọc cũ chưa bị mất dữ liệu.
            // Offset không rõ nguồn giữ NULL, không suy ra từ deadline đã thay đổi.
            migrationBuilder.Sql("""
                INSERT INTO lich_nhac (ma_nguoi_dung,ma_nhiem_vu,nhac_truoc_phut,moc_nhac,
                    thoi_diem_nhac,trang_thai,so_lan_thu,lan_thu_tiep_theo_luc,ngay_phat,
                    ma_thong_bao_cu,noi_dung,ngay_tao,ngay_cap_nhat)
                SELECT ma_nguoi_dung,ma_nhiem_vu,NULL,'LegacyDeadline',thoi_diem_nhac,
                    trang_thai_phat,so_lan_thu,lan_thu_tiep_theo_luc,ngay_phat,
                    ma,noi_dung,ngay_tao,ngay_cap_nhat FROM thong_bao;
                UPDATE thong_bao n SET ma_lich_nhac=r.ma FROM lich_nhac r
                    WHERE r.ma_thong_bao_cu=n.ma AND n.trang_thai_phat='Delivered';
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_nhiem_vu_hoc_ky_ma_hoc_ky_ma_nguoi_dung",
                table: "nhiem_vu",
                columns: new[] { "ma_hoc_ky", "ma_nguoi_dung" },
                principalTable: "hoc_ky",
                principalColumns: new[] { "ma", "ma_nguoi_dung" });

            migrationBuilder.AddForeignKey(
                name: "FK_nhiem_vu_mon_hoc_ma_mon_hoc_ma_hoc_ky",
                table: "nhiem_vu",
                columns: new[] { "ma_mon_hoc", "ma_hoc_ky" },
                principalTable: "mon_hoc",
                principalColumns: new[] { "ma", "ma_hoc_ky" });

            migrationBuilder.AddForeignKey(
                name: "FK_thong_bao_lich_nhac_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao",
                columns: new[] { "ma_lich_nhac", "ma_nguoi_dung", "ma_nhiem_vu" },
                principalTable: "lich_nhac",
                principalColumns: new[] { "ma", "ma_nguoi_dung", "ma_nhiem_vu" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_nhiem_vu_hoc_ky_ma_hoc_ky_ma_nguoi_dung",
                table: "nhiem_vu");

            migrationBuilder.DropForeignKey(
                name: "FK_nhiem_vu_mon_hoc_ma_mon_hoc_ma_hoc_ky",
                table: "nhiem_vu");

            migrationBuilder.DropForeignKey(
                name: "FK_thong_bao_lich_nhac_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao");

            migrationBuilder.DropTable(
                name: "doan_thuc_hoc");

            migrationBuilder.DropTable(
                name: "khung_gio_ranh");

            migrationBuilder.DropTable(
                name: "lich_nhac");

            migrationBuilder.DropTable(
                name: "quy_tac_uu_tien");

            migrationBuilder.DropTable(
                name: "tep_dinh_kem_nhiem_vu");

            migrationBuilder.DropTable(
                name: "phien_tu_hoc");

            migrationBuilder.DropTable(
                name: "lan_ap_dung_lich");

            migrationBuilder.DropIndex(
                name: "IX_thong_bao_ma_lich_nhac",
                table: "thong_bao");

            migrationBuilder.DropIndex(
                name: "IX_thong_bao_ma_lich_nhac_ma_nguoi_dung_ma_nhiem_vu",
                table: "thong_bao");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_nhiem_vu_ma_ma_nguoi_dung",
                table: "nhiem_vu");

            migrationBuilder.DropIndex(
                name: "IX_nhiem_vu_ma_hoc_ky_ma_nguoi_dung",
                table: "nhiem_vu");

            migrationBuilder.DropIndex(
                name: "IX_nhiem_vu_ma_mon_hoc_ma_hoc_ky",
                table: "nhiem_vu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_nhiem_vu_mon_hoc_ky",
                table: "nhiem_vu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_nhiem_vu_su_kien",
                table: "nhiem_vu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_nguoi_dung_revision",
                table: "nguoi_dung");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_mon_hoc_ma_ma_hoc_ky",
                table: "mon_hoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_lich_su_nguon",
                table: "lich_su_nhiem_vu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_lich_hoc_ngay_ap_dung",
                table: "lich_hoc");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_hoc_ky_ma_ma_nguoi_dung",
                table: "hoc_ky");

            migrationBuilder.DropColumn(
                name: "ma_lich_nhac",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "bat_dau_su_kien",
                table: "nhiem_vu");

            migrationBuilder.DropColumn(
                name: "ket_thuc_su_kien",
                table: "nhiem_vu");

            migrationBuilder.DropColumn(
                name: "ma_hoc_ky",
                table: "nhiem_vu");

            migrationBuilder.DropColumn(
                name: "phien_ban_lap_lich",
                table: "nguoi_dung");

            migrationBuilder.DropColumn(
                name: "nguon_thay_doi",
                table: "lich_su_nhiem_vu");

            migrationBuilder.DropColumn(
                name: "ngay_ap_dung_den",
                table: "lich_hoc");

            migrationBuilder.DropColumn(
                name: "ngay_ap_dung_tu",
                table: "lich_hoc");

            migrationBuilder.DropColumn(
                name: "da_luu_tru",
                table: "hoc_ky");

            migrationBuilder.AddForeignKey(
                name: "FK_task_items_courses_CourseId",
                table: "nhiem_vu",
                column: "ma_mon_hoc",
                principalTable: "mon_hoc",
                principalColumn: "ma",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
