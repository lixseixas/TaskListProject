using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskListProject.Application.Data;
using TaskListProject.Infrastructure.Data;

namespace TaskListProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LocalDbConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:LocalDbConnection is not configured");
        }

        services.AddDbContext<TaskContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ITasksQueries, TasksQueries>();
        services.AddScoped<ITasksCommands, TasksCommands>();
        services.AddScoped<IUserQueries, UserQueries>();
        services.AddScoped<IWeeklyTaskReportRepository, WeeklyTaskReportRepository>();
        services.AddScoped<IAccountMovementRepository, AccountMovementRepository>();
        return services;
    }
}
