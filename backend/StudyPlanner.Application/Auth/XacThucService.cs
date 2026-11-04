using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using StudyPlanner.Application.Administration;
using StudyPlanner.Application.Common;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Auth;

public sealed class XacThucService(IQuanLyNguoiDungStore store, INguoiDungPasswordHasher hasher,
    QuanLyNguoiDungService users, IAccessTokenIssuer tokens, TimeProvider clock)
{
    // Tài khoản không tồn tại vẫn xác minh hash để giảm khác biệt thời gian đăng nhập.
    private static string? dummyHash;

    public Task<NguoiDungResponse> DangKyAsync(DangKyRequest request, CancellationToken ct)
    {
        // Kiểm tra cả tại Application khi được gọi ngoài HTTP; tránh mất validation khi ánh xạ DTO.
        QuyTacTaiNguyen.Validate(request);
        return users.ThemAsync(new TaoNguoiDungRequest { Email = request.Email, HoTen = request.HoTen,
            MatKhau = request.MatKhau, MaSoSinhVien = request.MaSoSinhVien, TenTruong = request.TenTruong,
            TenLop = request.TenLop, NganhHoc = request.NganhHoc, DangHoatDong = true }, ct);
    }

    public async Task<DangNhapResponse> DangNhapAsync(DangNhapRequest request, CancellationToken ct)
    {
        QuyTacTaiNguyen.Validate(request);
        var identifier = request.TaiKhoan.Trim();
        var normalizedEmail = identifier.ToUpperInvariant();
        var matches = await store.LoadAsync<NguoiDung>(x => x.EmailChuanHoa == normalizedEmail ||
            x.MaSoSinhVien == identifier, ct);
        // Email không phân biệt hoa/thường; MSSV so đúng chuỗi đã trim theo unique hiện tại.
        // Nếu dữ liệu cũ có MSSV trùng email của tài khoản khác, từ chối thay vì chọn nhầm danh tính.
        var user = matches.Count == 1 ? matches[0] : null;
        // Dummy được cấp hash hợp lệ theo cùng thư viện, không bao giờ lưu DB.
        var candidate = user ?? new NguoiDung { MatKhauBam = LazyInitializer.EnsureInitialized(ref dummyHash,
            () => hasher.Hash(new NguoiDung(), Guid.NewGuid().ToString("N"))) };
        var valid = hasher.Verify(candidate, request.MatKhau);
        if (!valid || user is null || !user.DangHoatDong) throw new XacThucException();
        return tokens.Issue(user);
    }

    public Task<NguoiDungResponse> LayHoSoAsync(int id, CancellationToken ct) => users.LayChiTietAsync(id, ct);

    public async Task<NguoiDungResponse> CapNhatHoSoAsync(int id, JsonElement patch, CancellationToken ct)
    {
        if (patch.ValueKind != JsonValueKind.Object) throw InvalidProfile();
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await store.FindAsync<NguoiDung>(id, ct);
        if (user is null || !user.DangHoatDong) throw new XacThucException();
        var fullName = user.HoTen; var studentCode = user.MaSoSinhVien; var major = user.NganhHoc;
        var schoolName = user.TenTruong; var className = user.TenLop;
        foreach (var field in patch.EnumerateObject())
        {
            if (field.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) throw InvalidProfile();
            var value = field.Value.GetString()?.Trim();
            switch (field.Name)
            {
                case "hoTen": fullName = value ?? ""; break;
                case "maSoSinhVien": studentCode = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "tenTruong": schoolName = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "tenLop": className = string.IsNullOrWhiteSpace(value) ? null : value; break;
                case "nganhHoc": major = string.IsNullOrWhiteSpace(value) ? null : value; break;
                default: throw InvalidProfile();
            }
        }
        QuyTacTaiNguyen.Validate(new NguoiDungRequest { Email = user.Email, HoTen = fullName,
            MaSoSinhVien = studentCode, TenTruong = schoolName, TenLop = className,
            NganhHoc = major, DangHoatDong = user.DangHoatDong });
        if (studentCode != null && await store.AnyAsync<NguoiDung>(x => x.Ma != id && x.MaSoSinhVien == studentCode, ct))
            throw QuyTacTaiNguyen.Conflict("STUDENT_CODE_ALREADY_USED", "Mã sinh viên đã được sử dụng.");
        user.HoTen = fullName; user.MaSoSinhVien = studentCode; user.NganhHoc = major;
        user.TenTruong = schoolName; user.TenLop = className;
        user.NgayCapNhat = clock.GetUtcNow(); await store.SaveAsync(ct); await transaction.CommitAsync(ct);
        return await users.LayChiTietAsync(id, ct);
    }
    private static QuanTriException InvalidProfile() => new(LoaiLoiNghiepVu.InvalidInput, "VALIDATION_ERROR",
        "Chỉ sửa hoTen, maSoSinhVien, tenTruong, tenLop và nganhHoc với giá trị hợp lệ.");

    public async Task DoiMatKhauAsync(int id, DoiMatKhauRequest request, CancellationToken ct)
    {
        await using var transaction = await store.BeginTransactionAsync(ct);
        var user = await store.FindAsync<NguoiDung>(id, ct);
        if (user is null || !user.DangHoatDong || !hasher.Verify(user, request.MatKhauHienTai))
            throw new XacThucException();
        user.MatKhauBam = hasher.Hash(user, request.MatKhauMoi);
        user.NgayCapNhat = clock.GetUtcNow();
        await store.SaveAsync(ct); await transaction.CommitAsync(ct);
    }
}

