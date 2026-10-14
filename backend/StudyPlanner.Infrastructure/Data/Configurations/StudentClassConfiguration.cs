using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class StudentClassConfiguration : IEntityTypeConfiguration<StudentClass>
{
    public void Configure(EntityTypeBuilder<StudentClass> builder)
    {
        builder.ToTable("student_classes", table =>
        {
            table.HasCheckConstraint("CK_student_classes_name_not_blank", "length(btrim(\"Name\")) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => new { x.FacultyId, x.CohortId, x.Name }).IsUnique();
        builder.HasOne(x => x.Faculty).WithMany(x => x.StudentClasses).HasForeignKey(x => x.FacultyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cohort).WithMany(x => x.StudentClasses).HasForeignKey(x => x.CohortId).OnDelete(DeleteBehavior.Restrict);
    }
}

