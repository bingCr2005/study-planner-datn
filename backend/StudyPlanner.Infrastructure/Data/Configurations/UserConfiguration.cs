using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;
using StudyPlanner.Domain.Enums;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("CK_users_reminder_hours", "\"ReminderHours\" >= 0");
            table.HasCheckConstraint("CK_users_student_code", "\"StudentCode\" IS NULL OR length(btrim(\"StudentCode\")) > 0");
            table.HasCheckConstraint("CK_users_email_not_blank", "length(btrim(\"Email\")) > 0");
            table.HasCheckConstraint("CK_users_normalizedemail_not_blank", "length(btrim(\"NormalizedEmail\")) > 0");
            table.HasCheckConstraint("CK_users_fullname_not_blank", "length(btrim(\"FullName\")) > 0");
            table.HasCheckConstraint("CK_users_passwordhash_not_blank", "length(btrim(\"PasswordHash\")) > 0");
            table.HasCheckConstraint("CK_users_timezone_not_blank", "length(btrim(\"TimeZone\")) > 0");
            table.HasCheckConstraint("CK_users_role_values", "\"Role\" IN ('Student', 'Admin')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnType("text").IsRequired();
        builder.Property(x => x.StudentCode).HasMaxLength(50);
        builder.Property(x => x.Major).HasMaxLength(200);
        builder.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        // Application phải trim/chuẩn hóa email thống nhất trước khi lưu và khi đăng nhập.
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        // Theo giả định ERD: mã SV duy nhất trong đơn vị triển khai, không phải toàn cầu.
        builder.HasIndex(x => x.StudentCode).IsUnique().HasFilter("\"StudentCode\" IS NOT NULL");
        // Role phải do server kiểm soát; mặc định database không thay thế phân quyền API.
        builder.Property(x => x.Role).HasDefaultValue(UserRole.Student);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.TimeZone).HasDefaultValue("Asia/Ho_Chi_Minh");
        // 0 nghĩa là nhắc đúng hạn, không phải chưa gán; không để DB default 24 ghi đè.
        builder.Property(x => x.ReminderHours).HasDefaultValue(24).HasSentinel(24);
        builder.HasOne(x => x.Class).WithMany(x => x.Users).HasForeignKey(x => x.ClassId).OnDelete(DeleteBehavior.SetNull);
    }
}

