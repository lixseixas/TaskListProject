using Mapster;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.Mapping;

public static class MapsterConfig
{
    public static void ConfigureMapster()
    {
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

        // AccountMovementDto to AccountMovementModel mapping
        TypeAdapterConfig<AccountMovementDto, AccountMovementModel>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.Description, src => src.Description);

        // AccountMovementModel to AccountMovementDto mapping
        TypeAdapterConfig<AccountMovementModel, AccountMovementDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.Description, src => src.Description);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(MapsterConfig).Assembly);
    }
}
