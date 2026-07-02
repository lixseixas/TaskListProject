using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Models;

namespace TaskProject.Bl
{
    public class TaskContext : DbContext
    {
        public TaskContext(DbContextOptions<TaskContext> options) : base(options)
        {
        }       

        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<UserLoginModel> UserLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>().ToTable("Tasks");
            modelBuilder.Entity<UserLoginModel>().ToTable("UserLogin");
        }
    }
}
