using Mapster;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Mapping;

/// <summary>
/// Configures Mapster mappings for the application
/// </summary>
public static class MappingConfig
{
    /// <summary>
    /// Configures all type mappings for the application
    /// Call this method during application startup (e.g., in Program.cs)
    /// </summary>
    public static void Configure()
    {
        // Configure TaskDto mappings for deep copy operations
        TypeAdapterConfig<TaskDto, TaskDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Configure UserLoginDto mappings
        TypeAdapterConfig<UserLoginDto, UserLoginDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Configure WeeklyTaskReportDto mappings
        TypeAdapterConfig<WeeklyTaskReportDto, WeeklyTaskReportDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Configure SummarizedTasksDto mappings
        TypeAdapterConfig<SummarizedTasksDto, SummarizedTasksDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Configure TaskListDto mappings
        TypeAdapterConfig<TaskListDto, TaskListDto>
            .NewConfig()
            .IgnoreNullValues(true);

        // Configure SearchTaskDto mappings
        TypeAdapterConfig<SearchTaskDto, SearchTaskDto>
            .NewConfig()
            .IgnoreNullValues(true);
    }
}
