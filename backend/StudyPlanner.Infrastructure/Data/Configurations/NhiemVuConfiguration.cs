using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class NhiemVuConfiguration : IEntityTypeConfiguration<NhiemVu>
{
    public void Configure(EntityTypeBuilder<NhiemVu> builder)
    {
        builder.Property(x => x.MaHocKy).HasColumnName("ma_hoc_ky");
        builder.Property(x => x.BatDauSuKien).HasColumnName("bat_dau_su_kien");
        builder.Property(x => x.KetThucSuKien).HasColumnName("ket_thuc_su_kien");
        builder.Ignore(x => x.ThoiLuongConLaiPhut);
        builder.HasAlternateKey(x => new { x.Ma, x.MaNguoiDung });
        builder.ToTable("nhiem_vu", t => {
            t.HasCheckConstraint("CK_nhiem_vu_mon_hoc_ky", "ma_mon_hoc IS NULL OR ma_hoc_ky IS NOT NULL");
            t.HasCheckConstraint("CK_nhiem_vu_su_kien", "(bat_dau_su_kien IS NULL AND ket_thuc_su_kien IS NULL) OR (bat_dau_su_kien IS NOT NULL AND ket_thuc_su_kien IS NOT NULL AND ket_thuc_su_kien > bat_dau_su_kien)");
        });
        // Tên bảng/cột tiếng Việt; giữ tên constraint/index cũ để migration chỉ rename bảng.
        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.MaNguoiDung).HasColumnName("ma_nguoi_dung");
        builder.Property(x => x.MaMonHoc).HasColumnName("ma_mon_hoc");
        builder.Property(x => x.MaNhiemVuCha).HasColumnName("ma_nhiem_vu_cha");
        builder.Property(x => x.TieuDe).HasColumnName("tieu_de");
        builder.Property(x => x.MoTa).HasColumnName("mo_ta");
        builder.Property(x => x.Loai).HasColumnName("loai");
        builder.Property(x => x.HanHoanThanh).HasColumnName("han_hoan_thanh");
        builder.Property(x => x.MucDoUuTien).HasColumnName("muc_do_uu_tien");
        builder.Property(x => x.TrangThai).HasColumnName("trang_thai");
        builder.Property(x => x.ThoiLuongDuKienPhut).HasColumnName("thoi_luong_du_kien_phut");
        builder.Property(x => x.TienDo).HasColumnName("tien_do");
        builder.Property(x => x.NhacTruocGio).HasColumnName("nhac_truoc_gio");
        builder.Property(x => x.NgayHoanThanh).HasColumnName("ngay_hoan_thanh");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");

        builder.ToTable("nhiem_vu", table =>
        {
            table.HasCheckConstraint("CK_task_items_progress", "\"tien_do\" BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_task_items_estimated_minutes", "\"thoi_luong_du_kien_phut\" IS NULL OR \"thoi_luong_du_kien_phut\" > 0");
            table.HasCheckConstraint("CK_task_items_reminder_hours", "\"nhac_truoc_gio\" IS NULL OR \"nhac_truoc_gio\" >= 0");
            table.HasCheckConstraint("CK_task_items_parent", "\"ma_nhiem_vu_cha\" IS NULL OR \"ma_nhiem_vu_cha\" <> \"ma\"");
            table.HasCheckConstraint("CK_task_items_state", "(\"trang_thai\" = 'NotStarted' AND \"tien_do\" = 0 AND \"ngay_hoan_thanh\" IS NULL) OR (\"trang_thai\" = 'InProgress' AND \"tien_do\" BETWEEN 1 AND 99 AND \"ngay_hoan_thanh\" IS NULL) OR (\"trang_thai\" = 'Completed' AND \"tien_do\" = 100 AND \"ngay_hoan_thanh\" IS NOT NULL)");
            table.HasCheckConstraint("CK_task_items_title_not_blank", "length(btrim(\"tieu_de\")) > 0");
            table.HasCheckConstraint("CK_task_items_type_values", "\"loai\" IN ('Assignment', 'Exam', 'Revision', 'Personal')");
            table.HasCheckConstraint("CK_task_items_priority_values", "\"muc_do_uu_tien\" IN ('Low', 'Medium', 'High')");
            table.HasCheckConstraint("CK_task_items_status_values", "\"trang_thai\" IN ('NotStarted', 'InProgress', 'Completed')");
        });
        builder.HasKey(x => x.Ma).HasName("PK_task_items");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MoTa).HasColumnType("text");
        builder.Property(x => x.Loai).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.MucDoUuTien).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        // Low = 0 là giá trị CLR mặc định. Sentinel Medium giữ Low do người dùng chọn,
        // tránh EF hiểu Low là "chưa gán" rồi dùng mặc định database Medium.
        builder.Property(x => x.MucDoUuTien).HasDefaultValue(MucDoUuTienNhiemVu.Medium).HasSentinel(MucDoUuTienNhiemVu.Medium);
        builder.Property(x => x.TrangThai).HasDefaultValue(TrangThaiNhiemVu.NotStarted);
        builder.Property(x => x.TienDo).HasDefaultValue(0);
        builder.Property(x => x.HanHoanThanh).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayHoanThanh).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.MaNguoiDung, x.HanHoanThanh });
        builder.HasIndex(x => new { x.MaNguoiDung, x.TrangThai });
        builder.HasOne(x => x.NguoiDung).WithMany(x => x.DanhSachNhiemVu).HasForeignKey(x => x.MaNguoiDung).HasConstraintName("FK_task_items_users_UserId").OnDelete(DeleteBehavior.Cascade);
        // Xóa môn giữ nhiệm vụ. Application cần ghi lịch sử bỏ liên kết trước khi xóa môn.
        // Không SET NULL cả FK ghép vì sẽ xóa cả mã người dùng/học kỳ. Dịch vụ xóa bỏ liên kết trước.
        builder.HasOne(x => x.MonHoc).WithMany(x => x.DanhSachNhiemVu).HasForeignKey(x => new { x.MaMonHoc, x.MaHocKy }).HasPrincipalKey(x => new { x.Ma, x.MaHocKy }).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.HocKy).WithMany().HasForeignKey(x => new { x.MaHocKy, x.MaNguoiDung }).HasPrincipalKey(x => new { x.Ma, x.MaNguoiDung }).OnDelete(DeleteBehavior.NoAction);
        // Cascade chỉ xóa dữ liệu; không tự ghi TaskHistory hoặc tính lại tiến độ tổ tiên.
        builder.HasOne(x => x.NhiemVuCha).WithMany(x => x.DanhSachNhiemVuCon).HasForeignKey(x => x.MaNhiemVuCha).HasConstraintName("FK_task_items_task_items_ParentTaskId").OnDelete(DeleteBehavior.Cascade);
        // Giữ tên index hiện có để migration chỉ đổi tên cột.
        builder.HasIndex(x => x.MaMonHoc).HasDatabaseName("IX_task_items_CourseId");
        builder.HasIndex(x => x.MaNhiemVuCha).HasDatabaseName("IX_task_items_ParentTaskId");
        builder.HasIndex(x => new { x.MaNguoiDung, x.HanHoanThanh }).HasDatabaseName("IX_task_items_UserId_DueDate");
        builder.HasIndex(x => new { x.MaNguoiDung, x.TrangThai }).HasDatabaseName("IX_task_items_UserId_Status");
    }
}

