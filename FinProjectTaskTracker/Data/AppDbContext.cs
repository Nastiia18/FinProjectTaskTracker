using Microsoft.EntityFrameworkCore;
using FinProjectTaskTracker.Models;

namespace FinProjectTaskTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Board> Boards { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Assignee> Assignees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>()
            .HasIndex(b => b.Name)
            .IsUnique();
        
        modelBuilder.Entity<Board>()
            .Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Assignee>()
            .Property(a => a.Email)
            .IsRequired();
        
        modelBuilder.Entity<Board>()
            .HasMany<TaskItem>()
            .WithOne()
            .HasForeignKey(t => t.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<TaskItem>()
            .Property(t => t.Priority)
            .HasConversion<string>();
        
        modelBuilder.Entity<TaskItem>()
            .HasOne<Assignee>()
            .WithMany()
            .HasForeignKey(t => t.AssigneeId);
    }
}