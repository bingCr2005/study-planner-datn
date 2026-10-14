using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("class_sessions", table =>
        {
            table.HasCheckConstraint("CK_class_sessions_weekday", "\"DayOfWeek\" BETWEEN 1 AND 7");
            table.HasCheckConstraint("CK_class_sessions_times", "\"EndTime\" > \"StartTime\"");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Room).HasMaxLength(200);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.StartTime).HasColumnType("time without time zone");
        builder.Property(x => x.EndTime).HasColumnType("time without time zone");
        builder.HasIndex(x => new { x.CourseId, x.DayOfWeek, x.StartTime });
        builder.HasOne(x => x.Course).WithMany(x => x.ClassSessions).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}

