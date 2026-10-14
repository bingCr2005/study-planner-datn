using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("semesters", table =>
        {
            table.HasCheckConstraint("CK_semesters_dates", "\"EndDate\" >= \"StartDate\"");
            table.HasCheckConstraint("CK_semesters_name_not_blank", "length(btrim(\"Name\")) > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.HasIndex(x => new { x.UserId, x.StartDate });
        builder.HasOne(x => x.User).WithMany(x => x.Semesters).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

