using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Domain.Entities;

public class NhiemVu
{
    public int Ma { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaMonHoc { get; set; }
    // Việc chung học kỳ không cần môn. Có môn thì học kỳ phải đúng học kỳ của môn và đúng chủ.
    public int? MaHocKy { get; set; }
    // NULL là nhiệm vụ gốc. Chống vòng cây và kiểm tra cùng chủ sở hữu/môn ở Application.
    public int? MaNhiemVuCha { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public LoaiNhiemVu Loai { get; set; } = LoaiNhiemVu.Personal;
    // HanHoanThanh NULL có thể kế thừa hạn tổ tiên; hạn hiệu lực được tính, không lưu thêm cột.
    public DateTimeOffset? HanHoanThanh { get; set; }
    // Giờ một sự kiện (ví dụ ca thi), không phải thời gian tự học được phân bổ.
    public DateTimeOffset? BatDauSuKien { get; set; }
    public DateTimeOffset? KetThucSuKien { get; set; }
    public MucDoUuTienNhiemVu MucDoUuTien { get; set; } = MucDoUuTienNhiemVu.Medium;
    public TrangThaiNhiemVu TrangThai { get; set; } = TrangThaiNhiemVu.NotStarted;
    public int? ThoiLuongDuKienPhut { get; set; }
    public int TienDo { get; set; }
    public int? NhacTruocGio { get; set; }
    public DateTimeOffset? NgayHoanThanh { get; set; }
    public NguoiDung NguoiDung { get; set; } = null!;
    public MonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
    // Giá trị dẫn xuất, không lưu cột riêng để tránh lệch với tiến độ/ước lượng nguồn.
    public int? ThoiLuongConLaiPhut => ThoiLuongDuKienPhut is int minutes
        ? (int)Math.Ceiling(minutes * (100m - TienDo) / 100m) : null;
    public NhiemVu? NhiemVuCha { get; set; }
    public ICollection<NhiemVu> DanhSachNhiemVuCon { get; set; } = new List<NhiemVu>();
    // UNIQUE(MaNhiemVu, Loai) cùng CHECK Loai hiện tại giới hạn tối đa một lời nhắc mỗi nhiệm vụ.
    public ICollection<ThongBao> DanhSachThongBao { get; set; } = new List<ThongBao>();
    public DateTimeOffset NgayTao { get; set; }
    public DateTimeOffset NgayCapNhat { get; set; }
}

