using Microsoft.EntityFrameworkCore;

namespace LibreLMS.Api;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<StudentProfile> Students => Set<StudentProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Email).HasMaxLength(256);
            e.HasIndex(s => s.Email).IsUnique();
        });
    }
}
