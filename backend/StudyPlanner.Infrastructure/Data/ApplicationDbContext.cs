using Microsoft.EntityFrameworkCore;

namespace StudyPlanner.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Sau này khi tạo các Entity ở tầng Domain (User, Task, ...), ta sẽ khai báo DbSet ở đây
}