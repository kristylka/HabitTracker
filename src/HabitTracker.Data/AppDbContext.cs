namespace HabitTracker.Data;

using Microsoft.EntityFrameworkCore;
using HabitTracker.Core.Models;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<HabitEntry> HabitEntries => Set<HabitEntry>();
    public DbSet<HabitSchedule> HabitSchedules => Set<HabitSchedule>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Username).HasMaxLength(50).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Habit>(e =>
        {
            e.HasKey(h => h.Id);
            e.Property(h => h.Name).HasMaxLength(200).IsRequired();
            e.Property(h => h.Description).HasMaxLength(500);
            e.Property(h => h.Color).HasMaxLength(7);
            e.HasOne(h => h.User)
                .WithMany(u => u.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HabitEntry>(e =>
        {
            e.HasKey(he => he.Id);
            e.HasOne(he => he.Habit)
                .WithMany(h => h.Entries)
                .HasForeignKey(he => he.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HabitSchedule>(e =>
        {
            e.HasKey(hs => hs.Id);
            e.HasOne(hs => hs.Habit)
                .WithMany(h => h.Schedules)
                .HasForeignKey(hs => hs.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}