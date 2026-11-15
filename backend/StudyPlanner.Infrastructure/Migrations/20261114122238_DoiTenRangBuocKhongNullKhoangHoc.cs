using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DoiTenRangBuocKhongNullKhoangHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DoiTen(migrationBuilder, "doan_thuc_hoc", "khoang_thoi_gian_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DoiTen(migrationBuilder, "khoang_thoi_gian_hoc", "doan_thuc_hoc");
        }

        private static void DoiTen(MigrationBuilder builder, string tenCu, string tenMoi)
        {
            // PostgreSQL 18 lưu NOT NULL thành constraint có tên; bản cũ không có các dòng này.
            // Chỉ đổi tên khi tồn tại, giữ nguyên cột và dữ liệu trên cả hai phiên bản PostgreSQL.
            builder.Sql($$"""
                DO $rename_not_null$
                DECLARE item record;
                BEGIN
                    FOR item IN
                        SELECT conname FROM pg_constraint
                        WHERE conrelid = to_regclass('public.khoang_thoi_gian_hoc') AND contype = 'n'
                            AND starts_with(conname, '{{tenCu}}_')
                    LOOP
                        EXECUTE format('ALTER TABLE public.khoang_thoi_gian_hoc RENAME CONSTRAINT %I TO %I',
                            item.conname, '{{tenMoi}}' || substr(item.conname, length('{{tenCu}}') + 1));
                    END LOOP;
                END $rename_not_null$;
                """);
        }
    }
}
