using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.MaLop).HasDatabaseName("IX_users_ClassId");
        builder.Property(x => x.Ma).HasColumnName("ma");
        builder.Property(x => x.Email).HasColumnName("email");
        builder.Property(x => x.EmailChuanHoa).HasColumnName("email_chuan_hoa");
        builder.Property(x => x.HoTen).HasColumnName("ho_ten");
        builder.Property(x => x.MatKhauBam).HasColumnName("mat_khau_bam");
        builder.Property(x => x.MaSoSinhVien).HasColumnName("ma_so_sinh_vien");
        builder.Property(x => x.MaLop).HasColumnName("ma_lop");
        builder.Property(x => x.NganhHoc).HasColumnName("nganh_hoc");
        builder.Property(x => x.VaiTro).HasColumnName("vai_tro");
        builder.Property(x => x.DangHoatDong).HasColumnName("dang_hoat_dong");
        builder.Property(x => x.MuiGio).HasColumnName("mui_gio");
        builder.Property(x => x.NhacTruocGio).HasColumnName("nhac_truoc_gio");
        builder.Property(x => x.NgayTao).HasColumnName("ngay_tao");
        builder.Property(x => x.NgayCapNhat).HasColumnName("ngay_cap_nhat");
        builder.ToTable("nguoi_dung", table =>
        {
            table.HasCheckConstraint("CK_users_reminder_hours", "\"nhac_truoc_gio\" >= 0");
            table.HasCheckConstraint("CK_users_student_code", "\"ma_so_sinh_vien\" IS NULL OR length(btrim(\"ma_so_sinh_vien\")) > 0");
            table.HasCheckConstraint("CK_users_email_not_blank", "length(btrim(\"email\")) > 0");
            table.HasCheckConstraint("CK_users_normalizedemail_not_blank", "length(btrim(\"email_chuan_hoa\")) > 0");
            table.HasCheckConstraint("CK_users_fullname_not_blank", "length(btrim(\"ho_ten\")) > 0");
            table.HasCheckConstraint("CK_users_passwordhash_not_blank", "length(btrim(\"mat_khau_bam\")) > 0");
            table.HasCheckConstraint("CK_users_timezone_not_blank", "length(btrim(\"mui_gio\")) > 0");
            table.HasCheckConstraint("CK_users_role_values", "\"vai_tro\" IN ('Student', 'Admin')");
        });
        builder.Property(x=>x.TenTruong).HasColumnName("ten_truong").HasMaxLength(200);
        builder.Property(x=>x.TenLop).HasColumnName("ten_lop").HasMaxLength(200);
        builder.ToTable("nguoi_dung",t=> {
            t.HasCheckConstraint("CK_users_ten_truong_not_blank","\"ten_truong\" IS NULL OR \"ten_truong\" ~ '[^[:space:]]'");
            t.HasCheckConstraint("CK_users_ten_lop_not_blank","\"ten_lop\" IS NULL OR \"ten_lop\" ~ '[^[:space:]]'");
        });
        builder.HasKey(x => x.Ma).HasName("PK_users");
        builder.Property(x => x.Ma).UseIdentityByDefaultColumn();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EmailChuanHoa).HasMaxLength(320).IsRequired();
        builder.Property(x => x.HoTen).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MatKhauBam).HasColumnType("text").IsRequired();
        builder.Property(x => x.MaSoSinhVien).HasMaxLength(50);
        builder.Property(x => x.NganhHoc).HasMaxLength(200);
        builder.Property(x => x.MuiGio).HasMaxLength(100).IsRequired();
        builder.Property(x => x.VaiTro).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NgayTao).HasColumnType("timestamp with time zone");
        builder.Property(x => x.NgayCapNhat).HasColumnType("timestamp with time zone");
        // Application phải trim/chuẩn hóa email thống nhất trước khi lưu và khi đăng nhập.
        builder.HasIndex(x => x.EmailChuanHoa).HasDatabaseName("IX_users_NormalizedEmail").IsUnique();
        // Theo giả định ERD: mã SV duy nhất trong đơn vị triển khai, không phải toàn cầu.
        builder.HasIndex(x => x.MaSoSinhVien).HasDatabaseName("IX_users_StudentCode").IsUnique().HasFilter("\"ma_so_sinh_vien\" IS NOT NULL");
        // Role phải do server kiểm soát; mặc định database không thay thế phân quyền API.
        builder.Property(x => x.VaiTro).HasDefaultValue(UserRole.Student);
        builder.Property(x => x.DangHoatDong).HasDefaultValue(true);
        builder.Property(x => x.MuiGio).HasDefaultValue("Asia/Ho_Chi_Minh");
        // 0 nghĩa là nhắc đúng hạn, không phải chưa gán; không để DB default 24 ghi đè.
        builder.Property(x => x.NhacTruocGio).HasDefaultValue(24).HasSentinel(24);
        builder.HasOne(x => x.Class).WithMany(x => x.Users).HasForeignKey(x => x.MaLop).HasConstraintName("FK_users_student_classes_ClassId").OnDelete(DeleteBehavior.SetNull);
    }
}

