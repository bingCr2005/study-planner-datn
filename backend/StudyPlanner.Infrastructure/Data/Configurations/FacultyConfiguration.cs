using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Infrastructure.Data.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("faculties", table =>
        {
            table.HasCheckConstraint("CK_faculties_code_not_blank", "length(btrim(\"Code\")) > 0");
            table.HasCheckConstraint("CK_faculties_name_not_blank", "length(btrim(\"Name\")) > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Mã khoa được lặp ở trường khác, nhưng không được trùng trong cùng trường.
        builder.HasIndex(x => new { x.SchoolId, x.Code })
            .IsUnique();

        builder.HasOne(x => x.School)
            .WithMany(x => x.Faculties)
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Restrict); // Còn khoa thì không được xóa trường.
    }
}
