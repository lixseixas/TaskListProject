using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Domain.Entities;

namespace TaskListProject.Infrastructure.Data
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {       
        }

        public DbSet<TaskDto> Tasks { get; set; }
        public DbSet<UserLoginDto> UserLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskDto>().ToTable("Tasks");
            modelBuilder.Entity<UserLoginDto>().ToTable("UserLogin");
            modelBuilder.Entity<WeeklyTaskReportDto>().ToTable("WeeklyTaskReports");
        }
    }
}
