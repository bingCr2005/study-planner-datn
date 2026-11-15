using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyPlanner.Infrastructure.Migrations;

/// <summary>
/// Đổi cách tính tiến độ cha và giữ checklist riêng của cha bằng một nhiệm vụ con thực sự.
/// Phải chuyển dữ liệu trước khi cho phép ghi theo quy tắc nhiệm vụ mới.
/// </summary>
public partial class TongHopTienDoNhiemVu : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- EF executes the whole migration in a transaction. Take locks in the same owner -> task order
            -- as application writes; never expose an intermediate tree without its preserved checklist.
            LOCK TABLE nguoi_dung, nhiem_vu IN SHARE ROW EXCLUSIVE MODE;

            CREATE FUNCTION pg_temp.sp_progress_snapshot(n nhiem_vu) RETURNS jsonb
            LANGUAGE sql STABLE AS $snapshot$
                SELECT jsonb_build_object(
                    'tieuDe', n.tieu_de, 'moTa', n.mo_ta, 'checklist', COALESCE(n.checklist, '[]'::jsonb),
                    'maNhiemVuCha', n.ma_nhiem_vu_cha, 'maHocKy', n.ma_hoc_ky, 'maMonHoc', n.ma_mon_hoc,
                    'loai', n.loai, 'mucDoUuTien', n.muc_do_uu_tien, 'hanHoanThanh', n.han_hoan_thanh,
                    'batDauSuKien', n.bat_dau_su_kien, 'ketThucSuKien', n.ket_thuc_su_kien,
                    'tienDo', n.tien_do, 'trangThai', n.trang_thai,
                    'thoiLuongDuKienPhut', n.thoi_luong_du_kien_phut, 'nhacTruocGio', n.nhac_truoc_gio,
                    'ngayHoanThanh', n.ngay_hoan_thanh);
            $snapshot$;

            -- Deleted groups are independent forests. A deleted group's root is detached on trash;
            -- its historical ma_cha_truoc_khi_xoa is deliberately not a live parent edge.
            DO $preflight$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM nhiem_vu n LEFT JOIN nhiem_vu p ON p.ma = n.ma_nhiem_vu_cha
                    WHERE n.ma_nhiem_vu_cha IS NOT NULL AND (
                        p.ma IS NULL OR p.ma_nguoi_dung <> n.ma_nguoi_dung OR
                        p.ma_hoc_ky IS DISTINCT FROM n.ma_hoc_ky OR p.ma_mon_hoc IS DISTINCT FROM n.ma_mon_hoc OR
                        (n.ngay_xoa IS NULL) <> (p.ngay_xoa IS NULL) OR
                        (n.ngay_xoa IS NOT NULL AND (n.ma_nhom_xoa IS NULL OR n.ma_nhom_xoa IS DISTINCT FROM p.ma_nhom_xoa))))
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_INVALID_TREE: repair task owner/scope/trash-group links before upgrading.';
                END IF;
                IF EXISTS (SELECT 1 FROM nhiem_vu WHERE checklist IS NOT NULL AND jsonb_typeof(checklist) <> 'array')
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_INVALID_CHECKLIST: checklist must be a JSON array.';
                END IF;
                IF EXISTS (
                    SELECT 1 FROM nhiem_vu n WHERE n.checklist IS NOT NULL AND
                    (jsonb_array_length(n.checklist) > 100 OR EXISTS (
                        SELECT 1 FROM jsonb_array_elements(n.checklist) item
                        WHERE jsonb_typeof(item) <> 'object' OR jsonb_typeof(item->'tieuDe') IS DISTINCT FROM 'string' OR
                            length(btrim(item->>'tieuDe')) NOT BETWEEN 1 AND 200 OR
                            jsonb_typeof(item->'daXong') IS DISTINCT FROM 'boolean')))
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_INVALID_CHECKLIST: repair invalid checklist items before upgrading.';
                END IF;
            END $preflight$;

            CREATE TEMP TABLE sp_progress_depth ON COMMIT DROP AS
                WITH RECURSIVE tree AS (
                    SELECT ma, ma AS root, 1 AS depth, ARRAY[ma] AS path FROM nhiem_vu WHERE ma_nhiem_vu_cha IS NULL
                    UNION ALL
                    SELECT n.ma, tree.root, tree.depth + 1, tree.path || n.ma
                    FROM nhiem_vu n JOIN tree ON n.ma_nhiem_vu_cha = tree.ma
                    WHERE NOT n.ma = ANY(tree.path) AND tree.depth <= 10
                ) SELECT ma, root, depth FROM tree;

            CREATE TEMP TABLE sp_progress_preserve ON COMMIT DROP AS
                SELECT n.ma FROM nhiem_vu n
                WHERE n.checklist IS NOT NULL AND jsonb_array_length(n.checklist) > 0
                    AND EXISTS (SELECT 1 FROM nhiem_vu child WHERE child.ma_nhiem_vu_cha = n.ma);

            DO $bounds$
            BEGIN
                IF (SELECT count(*) FROM sp_progress_depth) <> (SELECT count(*) FROM nhiem_vu)
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_INVALID_TREE: cycle or missing root; repair task tree before upgrading.';
                END IF;
                IF EXISTS (SELECT 1 FROM sp_progress_depth WHERE depth > 10) OR
                    EXISTS (SELECT 1 FROM sp_progress_depth d JOIN sp_progress_preserve p ON p.ma = d.ma WHERE d.depth >= 10)
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_TREE_LIMIT: preserving parent checklists would exceed 10 levels; split the tree before upgrading.';
                END IF;
                IF EXISTS (
                    SELECT 1 FROM sp_progress_depth d LEFT JOIN sp_progress_preserve p ON p.ma = d.ma
                    GROUP BY d.root HAVING count(*) + count(p.ma) > 1000)
                THEN RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_TREE_LIMIT: preserving parent checklists would exceed 1000 tasks; split the tree before upgrading.';
                END IF;
            END $bounds$;

            CREATE TEMP TABLE sp_progress_before ON COMMIT DROP AS
                SELECT ma, ma_nguoi_dung, pg_temp.sp_progress_snapshot(n) AS snapshot FROM nhiem_vu n;
            CREATE TEMP TABLE sp_progress_created (ma integer PRIMARY KEY, parent integer NOT NULL) ON COMMIT DROP;

            WITH inserted AS (
                INSERT INTO nhiem_vu (
                    ma_nguoi_dung, ma_hoc_ky, ma_mon_hoc, ma_nhiem_vu_cha, tieu_de, mo_ta, checklist,
                    loai, muc_do_uu_tien, han_hoan_thanh, bat_dau_su_kien, ket_thuc_su_kien,
                    tien_do, trang_thai, thoi_luong_du_kien_phut, nhac_truoc_gio, ngay_hoan_thanh,
                    ngay_xoa, ma_nhom_xoa, ma_cha_truoc_khi_xoa, ngay_tao, ngay_cap_nhat)
                SELECT n.ma_nguoi_dung, n.ma_hoc_ky, n.ma_mon_hoc, n.ma, 'Công việc chung',
                    NULL, n.checklist, n.loai, n.muc_do_uu_tien, NULL, NULL, NULL,
                    result.progress,
                    CASE WHEN result.progress = 100 THEN 'Completed' WHEN result.progress = 0 THEN 'NotStarted' ELSE 'InProgress' END,
                    n.thoi_luong_du_kien_phut, NULL,
                    CASE WHEN result.progress = 100 THEN COALESCE(n.ngay_hoan_thanh, CURRENT_TIMESTAMP) ELSE NULL END,
                    n.ngay_xoa, n.ma_nhom_xoa, NULL, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                FROM nhiem_vu n JOIN sp_progress_preserve p ON p.ma = n.ma
                CROSS JOIN LATERAL (
                    -- PostgreSQL may evaluate this lateral aggregate before the preservation join.
                    -- Rows without a checklist must be safe even though they are not ultimately inserted.
                    SELECT (count(*) FILTER (WHERE (item->>'daXong')::boolean) * 100 / NULLIF(count(*), 0))::integer AS progress
                    FROM jsonb_array_elements(n.checklist) item
                ) result
                RETURNING ma, ma_nhiem_vu_cha
            ) INSERT INTO sp_progress_created SELECT ma, ma_nhiem_vu_cha FROM inserted;

            INSERT INTO sp_progress_depth
                SELECT c.ma, d.root, d.depth + 1 FROM sp_progress_created c JOIN sp_progress_depth d ON d.ma = c.parent;

            UPDATE nhiem_vu n SET checklist = NULL, thoi_luong_du_kien_phut = NULL, ngay_cap_nhat = CURRENT_TIMESTAMP
                WHERE EXISTS (SELECT 1 FROM nhiem_vu child WHERE child.ma_nhiem_vu_cha = n.ma)
                    AND (n.checklist IS NOT NULL OR n.thoi_luong_du_kien_phut IS NOT NULL);

            -- Normalize every checklist leaf too; a leaf without a checklist retains its manually recorded progress.
            WITH derived AS (
                SELECT n.ma, (count(*) FILTER (WHERE (item->>'daXong')::boolean) * 100 / NULLIF(count(*), 0))::integer AS progress
                FROM nhiem_vu n CROSS JOIN LATERAL jsonb_array_elements(n.checklist) item
                WHERE n.checklist IS NOT NULL AND jsonb_array_length(n.checklist) > 0
                GROUP BY n.ma
            )
            UPDATE nhiem_vu n SET tien_do = v.progress,
                trang_thai = CASE WHEN v.progress = 100 THEN 'Completed' WHEN v.progress = 0 THEN 'NotStarted' ELSE 'InProgress' END,
                ngay_hoan_thanh = CASE WHEN v.progress = 100 THEN COALESCE(n.ngay_hoan_thanh, CURRENT_TIMESTAMP) ELSE NULL END,
                ngay_cap_nhat = CURRENT_TIMESTAMP
            FROM derived v WHERE n.ma = v.ma AND (n.tien_do IS DISTINCT FROM v.progress);

            -- Process deepest parents first. Integer division is deliberate: an incomplete descendant can never round to 100%.
            DO $rollup$
            DECLARE level integer;
            BEGIN
                FOR level IN REVERSE 10..1 LOOP
                    WITH derived AS (
                        SELECT p.ma, (sum(child.tien_do)::bigint / NULLIF(count(*), 0))::integer AS progress
                        FROM nhiem_vu p JOIN sp_progress_depth d ON d.ma = p.ma
                        JOIN nhiem_vu child ON child.ma_nhiem_vu_cha = p.ma
                        WHERE d.depth = level GROUP BY p.ma
                    )
                    UPDATE nhiem_vu n SET tien_do = v.progress,
                        trang_thai = CASE WHEN v.progress = 100 THEN 'Completed' WHEN v.progress = 0 THEN 'NotStarted' ELSE 'InProgress' END,
                        ngay_hoan_thanh = CASE WHEN v.progress = 100 THEN COALESCE(n.ngay_hoan_thanh, CURRENT_TIMESTAMP) ELSE NULL END,
                        ngay_cap_nhat = CURRENT_TIMESTAMP
                    FROM derived v WHERE n.ma = v.ma AND n.tien_do IS DISTINCT FROM v.progress;
                END LOOP;
            END $rollup$;

            CREATE TEMP TABLE sp_progress_changed ON COMMIT DROP AS
                SELECT n.ma, n.ma_nguoi_dung, n.tieu_de, b.snapshot AS before, pg_temp.sp_progress_snapshot(n) AS after
                FROM nhiem_vu n JOIN sp_progress_before b ON b.ma = n.ma
                WHERE b.snapshot IS DISTINCT FROM pg_temp.sp_progress_snapshot(n);

            -- Newly completed parents must not send an outstanding reminder. Delivered inbox entries
            -- remain history; previously cancelled reminders are not silently resurrected on reopening.
            UPDATE lich_nhac r SET trang_thai = 'Cancelled', ngay_phat = NULL,
                lan_thu_tiep_theo_luc = NULL, ngay_cap_nhat = CURRENT_TIMESTAMP
                WHERE r.trang_thai IN ('Pending', 'Failed') AND EXISTS (
                    SELECT 1 FROM sp_progress_changed c WHERE c.ma = r.ma_nhiem_vu
                        AND c.before->>'trangThai' <> 'Completed' AND c.after->>'trangThai' = 'Completed');
            UPDATE thong_bao r SET trang_thai_phat = 'Cancelled', ngay_phat = NULL,
                lan_thu_tiep_theo_luc = NULL, ngay_cap_nhat = CURRENT_TIMESTAMP
                WHERE r.trang_thai_phat IN ('Pending', 'Failed') AND EXISTS (
                    SELECT 1 FROM sp_progress_changed c WHERE c.ma = r.ma_nhiem_vu
                        AND c.before->>'trangThai' <> 'Completed' AND c.after->>'trangThai' = 'Completed');

            INSERT INTO lich_su_nhiem_vu (ma_nhiem_vu, ma_nguoi_dung, hanh_dong, nguon_thay_doi, tieu_de_nhiem_vu, du_lieu_truoc, du_lieu_sau, ngay_thay_doi)
                SELECT ma, ma_nguoi_dung,
                    CASE WHEN before->>'trangThai' <> 'Completed' AND after->>'trangThai' = 'Completed' THEN 'Completed'
                         WHEN before->>'trangThai' = 'Completed' AND after->>'trangThai' <> 'Completed' THEN 'Reopened' ELSE 'Updated' END,
                    'System', tieu_de, before, after, CURRENT_TIMESTAMP FROM sp_progress_changed;
            INSERT INTO lich_su_nhiem_vu (ma_nhiem_vu, ma_nguoi_dung, hanh_dong, nguon_thay_doi, tieu_de_nhiem_vu, du_lieu_truoc, du_lieu_sau, ngay_thay_doi)
                SELECT n.ma, n.ma_nguoi_dung, 'Created', 'System', n.tieu_de, NULL, pg_temp.sp_progress_snapshot(n), CURRENT_TIMESTAMP
                FROM nhiem_vu n JOIN sp_progress_created c ON c.ma = n.ma;
            UPDATE nguoi_dung n SET phien_ban_lap_lich = n.phien_ban_lap_lich + 1
                WHERE n.ma IN (SELECT ma_nguoi_dung FROM sp_progress_changed
                    UNION SELECT n.ma_nguoi_dung FROM nhiem_vu n JOIN sp_progress_created c ON c.ma = n.ma);

            DROP FUNCTION pg_temp.sp_progress_snapshot(nhiem_vu);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Các nhiệm vụ con này có thể đã được sửa, đặt lịch hoặc ghi thực học sau nâng cấp; xóa sẽ mất công việc.
        migrationBuilder.Sql("""
            DO $rollback$ BEGIN
                RAISE EXCEPTION 'TASK_PROGRESS_MIGRATION_NO_AUTOMATIC_DOWNGRADE: restore a verified pre-upgrade backup to revert task progress semantics; automatic rollback would discard preserved task work.';
            END $rollback$;
            """);
    }
}
