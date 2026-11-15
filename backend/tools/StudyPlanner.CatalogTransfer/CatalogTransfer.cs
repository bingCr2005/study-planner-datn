using System.Text.Json;
using Npgsql;
using StudyPlanner.Infrastructure.NenTang;

public static class CatalogTransfer
{
    public const string LegacyMigration = "20261113102702_ThemDanhMucHocTapMau";
    public static async Task<string> RunAsync(string connectionString, DanhMucJsonStore store, bool apply, CancellationToken ct = default)
    {
        await using var fileLock = await store.KhoaAsync(ct);
        await using var connection = new NpgsqlConnection(connectionString); await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        async Task<object?> Scalar(string sql) { await using var cmd = new NpgsqlCommand(sql, connection, tx); return await cmd.ExecuteScalarAsync(ct); }
        async Task Execute(string sql) { await using var cmd = new NpgsqlCommand(sql, connection, tx); await cmd.ExecuteNonQueryAsync(ct); }
        await Execute("SET LOCAL lock_timeout = '10s'; SET LOCAL statement_timeout = '30s';");
        var exists = (bool)(await Scalar("SELECT to_regclass('public.hoc_ky_mau') IS NOT NULL AND to_regclass('public.mon_hoc_mau') IS NOT NULL"))!;
        if (!exists)
        {
            var any = (bool)(await Scalar("SELECT to_regclass('public.hoc_ky_mau') IS NOT NULL OR to_regclass('public.mon_hoc_mau') IS NOT NULL"))!;
            if (any) throw new InvalidOperationException("Legacy catalog is incomplete; no table was removed.");
            if (File.Exists(store.FilePath)) DanhMucJsonStore.Parse(await File.ReadAllTextAsync(store.FilePath, ct));
            return "No legacy catalog tables. JSON remains unchanged.";
        }
        await Execute("SELECT pg_advisory_xact_lock(73011301); LOCK TABLE \"__EFMigrationsHistory\", hoc_ky_mau, mon_hoc_mau IN ACCESS EXCLUSIVE MODE;");
        var latest = (string?)(await Scalar("SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1"));
        if (latest != LegacyMigration) throw new InvalidOperationException("Catalog migration is not the latest migration. Refusing to change database history.");
        var data = new DanhMucJson();
        await using (var cmd = new NpgsqlCommand("SELECT ma,ten,thu_tu,phien_ban FROM hoc_ky_mau ORDER BY ma", connection, tx))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
            while (await reader.ReadAsync(ct)) data.HocKy.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3)));
        await using (var cmd = new NpgsqlCommand("SELECT ma,ma_hoc_ky_mau,ky_hieu,ten,so_tin_chi,phien_ban FROM mon_hoc_mau ORDER BY ma", connection, tx))
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
            while (await reader.ReadAsync(ct)) data.MonHoc.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4), reader.GetInt32(5)));
        data.MaHocKyTiepTheo = checked((int)(long)(await Scalar("SELECT last_value + CASE WHEN is_called THEN 1 ELSE 0 END FROM hoc_ky_mau_ma_seq"))!);
        data.MaMonTiepTheo = checked((int)(long)(await Scalar("SELECT last_value + CASE WHEN is_called THEN 1 ELSE 0 END FROM mon_hoc_mau_ma_seq"))!);
        DanhMucJsonStore.KiemTra(data);
        var exported = JsonSerializer.Serialize(data, DanhMucJsonStore.Json);
        if (File.Exists(store.FilePath) && JsonSerializer.Serialize(DanhMucJsonStore.Parse(await File.ReadAllTextAsync(store.FilePath, ct)), DanhMucJsonStore.Json) != exported)
            throw new InvalidOperationException("Existing JSON differs from database. Refusing to overwrite either copy; reconcile catalog first.");
        var backupDir = Path.Combine(Path.GetDirectoryName(store.FilePath)!, "backups"); Directory.CreateDirectory(backupDir);
        var backup = Path.Combine(backupDir, "catalog-before-transfer-" + Guid.NewGuid().ToString("N") + ".json");
        await using (var stream = new FileStream(backup, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        { await JsonSerializer.SerializeAsync(stream, data, DanhMucJsonStore.Json, ct); await stream.FlushAsync(ct); stream.Flush(true); }
        await store.LuuTrongKhoaAsync(data, ct);
        if (JsonSerializer.Serialize(DanhMucJsonStore.Parse(await File.ReadAllTextAsync(store.FilePath, ct)), DanhMucJsonStore.Json) != exported ||
            JsonSerializer.Serialize(DanhMucJsonStore.Parse(await File.ReadAllTextAsync(backup, ct)), DanhMucJsonStore.Json) != exported)
            throw new InvalidOperationException("Export verification failed; database unchanged.");
        if (apply)
        {
            // Exact Down of the uncommitted migration. No CASCADE, no unrelated table/history changes.
            await Execute("DROP TABLE mon_hoc_mau; DROP TABLE hoc_ky_mau; DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '20261113102702_ThemDanhMucHocTapMau';");
        }
        await tx.CommitAsync(ct);
        return $"PASS: exported and verified {data.HocKy.Count} semesters/{data.MonHoc.Count} courses, IDs, versions and sequence counters. Backup: {backup}. Legacy tables " + (apply ? "removed." : "unchanged.");
    }
}
