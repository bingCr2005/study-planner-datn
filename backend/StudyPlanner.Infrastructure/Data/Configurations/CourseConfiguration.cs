using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses", table =>
        {
            table.HasCheckConstraint("CK_courses_credits", "\"Credits\" IS NULL OR \"Credits\" > 0");
            table.HasCheckConstraint("CK_courses_color", "\"Color\" IS NULL OR \"Color\" ~ '^#[0-9A-Fa-f]{6}$'");
            table.HasCheckConstraint("CK_courses_name_not_blank", "length(btrim(\"Name\")) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Lecturer).HasMaxLength(200);
        builder.Property(x => x.Color).HasMaxLength(7);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.HasOne(x => x.Semester).WithMany(x => x.Courses).HasForeignKey(x => x.SemesterId).OnDelete(DeleteBehavior.Cascade);
    }
}

