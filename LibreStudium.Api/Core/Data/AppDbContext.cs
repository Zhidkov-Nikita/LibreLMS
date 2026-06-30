using Microsoft.EntityFrameworkCore;

namespace LibreStudium.Api;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public IQueryable<User> Students => Users.Where(u => u.Role == Role.Student);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<StudentProfile>(e =>
        {
            e.HasKey(sp => sp.UserId);
            e.Property(sp => sp.FirstName).HasMaxLength(100);
            e.Property(sp => sp.LastName).HasMaxLength(100);

            e.HasOne(sp => sp.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<StudentProfile>(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
