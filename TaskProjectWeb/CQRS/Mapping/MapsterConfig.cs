using Mapster;
using TaskProject.Domain.Entities;
using TaskProject.Models;

namespace TaskProject.CQRS.Mapping;

public static class MapsterConfig
{
    public static void ConfigureMapster()
    {
        // TaskDto to TaskModel mapping
        TypeAdapterConfig<TaskDto, TaskModel>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.InitialHour, src => src.InitialHour)
            .Map(dest => dest.FinalHour, src => src.FinalHour)
            .Map(dest => dest.Priority, src => src.Priority)
            .Map(dest => dest.Ended, src => src.Ended);

        // TaskModel to TaskDto mapping
        TypeAdapterConfig<TaskModel, TaskDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.InitialHour, src => src.InitialHour)
            .Map(dest => dest.FinalHour, src => src.FinalHour)
            .Map(dest => dest.Priority, src => src.Priority)
            .Map(dest => dest.Ended, src => src.Ended);

        // SummarizedTasksDto to SummarizedTasksModel mapping
        TypeAdapterConfig<SummarizedTasksDto, SummarizedTasksModel>
            .NewConfig()
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.Hours, src => src.Hours)
            .Map(dest => dest.TotalTasks, src => src.TotalTasks)
            .Map(dest => dest.AverageHours, src => src.AverageHours)
            .Map(dest => dest.PercentualConcludedTasks, src => src.PercentualConcludedTasks);

        // SummarizedTasksModel to SummarizedTasksDto mapping
        TypeAdapterConfig<SummarizedTasksModel, SummarizedTasksDto>
            .NewConfig()
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.Hours, src => src.Hours)
            .Map(dest => dest.TotalTasks, src => src.TotalTasks)
            .Map(dest => dest.AverageHours, src => src.AverageHours)
            .Map(dest => dest.PercentualConcludedTasks, src => src.PercentualConcludedTasks);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterConfig).Assembly);
    }
}
