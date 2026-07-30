using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskListProject.Infrastructure.Data
{
    public class TaskContextDesignTimeFactory : IDesignTimeDbContextFactory<TaskContext>
    {
        public TaskContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("LocalDbConnection");

            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TaskContext(optionsBuilder.Options);
        }
    }
}
