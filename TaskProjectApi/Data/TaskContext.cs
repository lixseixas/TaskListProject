using Microsoft.EntityFrameworkCore;
using TaskReportApi.Models;

namespace TaskReportApi.Data;

/// <summary>
/// Database context for task management
/// </summary>
public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<WeeklyTaskReportModel> WeeklyTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });

        modelBuilder.Entity<WeeklyTaskReportModel>().ToTable("WeeklyTaskReports");
    }
}
