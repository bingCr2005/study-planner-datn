using System.Linq.Expressions;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;
namespace StudyPlanner.Application.HocTap;

public static class QuyTacCay
{
    public const int DoSauToiDa = 10, SoNodeToiDa = 1000;
    public static Expression<Func<NhiemVu, DateTimeOffset?>> BieuThucHan { get; } = TaoBieuThucHan();
    private static Expression<Func<NhiemVu, DateTimeOffset?>> TaoBieuThucHan()
    {
        var p = Expression.Parameter(typeof(NhiemVu), "n"); Expression node = p;
        Expression? result = null;
        for (var i = 0; i < DoSauToiDa; i++)
        {
            Expression han = Expression.Property(node, nameof(NhiemVu.HanHoanThanh));
            result = result is null ? han : Expression.Coalesce(result, han);
            node = Expression.Property(node, nameof(NhiemVu.NhiemVuCha));
        }
        return Expression.Lambda<Func<NhiemVu, DateTimeOffset?>>(result!, p);
    }
    public static IQueryable<NhiemVu> LocHan(IQueryable<NhiemVu> query, DateTimeOffset? tu, DateTimeOffset? den, bool khongHan = false)
    {
        var p = BieuThucHan.Parameters[0]; var han = BieuThucHan.Body;
        Expression condition = khongHan ? Expression.Equal(han, Expression.Constant(null, typeof(DateTimeOffset?))) : Expression.Constant(true);
        if (tu.HasValue) condition = Expression.AndAlso(condition, Expression.GreaterThanOrEqual(han, Expression.Constant(tu, typeof(DateTimeOffset?))));
        if (den.HasValue) condition = Expression.AndAlso(condition, Expression.LessThan(han, Expression.Constant(den, typeof(DateTimeOffset?))));
        return query.Where(Expression.Lambda<Func<NhiemVu, bool>>(condition, p));
    }
    public static (DateTimeOffset? Han, int? Nguon) HanHieuLuc(NhiemVu node, IReadOnlyDictionary<int, NhiemVu> map)
    {
        var seen = new HashSet<int>();
        while (true)
        {
            if (!seen.Add(node.Ma)) throw new HocTapException(409, "TASK_CYCLE", "Cây nhiệm vụ có vòng.");
            if (node.HanHoanThanh.HasValue) return (node.HanHoanThanh, node.Ma);
            if (node.MaNhiemVuCha is not int id) return (null, null);
            if (!map.TryGetValue(id, out var parent)) throw new HocTapException(409, "TASK_TREE_INVALID", "Không đọc được đầy đủ cây nhiệm vụ.");
            node = parent;
        }
    }
    public static void KiemTra(IReadOnlyList<NhiemVu> nodes)
    {
        if (nodes.Count > SoNodeToiDa) throw new HocTapException(409, "TASK_TREE_LIMIT", "Cây tối đa 1.000 nhiệm vụ.");
        var map = nodes.ToDictionary(x => x.Ma);
        foreach (var n in nodes)
        {
            var seen = new HashSet<int>(); var current = n; var depth = 1;
            while (current.MaNhiemVuCha is int id)
            {
                if (!seen.Add(current.Ma) || id == n.Ma) throw new HocTapException(409, "TASK_CYCLE", "Không được tạo vòng trong cây nhiệm vụ.");
                if (!map.TryGetValue(id, out var parent)) throw new HocTapException(409, "TASK_TREE_INVALID", "Cây không đầy đủ.");
                if (current.MaNguoiDung != parent.MaNguoiDung || current.MaHocKy != parent.MaHocKy || current.MaMonHoc != parent.MaMonHoc)
                    throw new HocTapException(409, "TASK_SCOPE_CONFLICT", "Cha và con phải cùng chủ, học kỳ và môn.");
                if (++depth > DoSauToiDa) throw new HocTapException(409, "TASK_TREE_LIMIT", "Cây tối đa 10 cấp.");
                current = parent;
            }
            if (n.MaNhiemVuCha is int pid && n.HanHoanThanh is DateTimeOffset own && HanHieuLuc(map[pid], map).Han is DateTimeOffset ph && own > ph)
                throw new HocTapException(409, "TASK_DEADLINE_CONFLICT", "Hạn nhiệm vụ con không được muộn hơn hạn của cha.", new { maNhiemVu = n.Ma });
        }
    }
    public static void TinhLai(IReadOnlyList<NhiemVu> nodes, DateTimeOffset now)
    {
        var children = nodes.Where(x => x.MaNhiemVuCha.HasValue).ToLookup(x => x.MaNhiemVuCha!.Value);
        var roots = nodes.Where(x => !x.MaNhiemVuCha.HasValue).ToArray();
        void Visit(NhiemVu n)
        {
            var direct = children[n.Ma].ToArray(); foreach (var child in direct) Visit(child);
            if (direct.Length == 0) return;
            var progress = direct.All(x => x.TienDo == 100) ? 100 : direct.All(x => x.TienDo == 0) ? 0 : Math.Clamp((int)direct.Average(x => x.TienDo), 1, 99);
            if (n.TienDo != progress) n.NgayCapNhat = now;
            n.TienDo = progress; n.ThoiLuongDuKienPhut = null;
            n.TrangThai = progress == 100 ? TrangThaiNhiemVu.Completed : progress == 0 ? TrangThaiNhiemVu.NotStarted : TrangThaiNhiemVu.InProgress;
            n.NgayHoanThanh = progress == 100 ? n.NgayHoanThanh ?? now : null;
        }
        foreach (var root in roots) Visit(root);
    }
}
