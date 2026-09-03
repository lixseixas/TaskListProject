using Mapster;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.Mapping;

public static class MapsterConfig
{
    public static void ConfigureMapster()
    {      

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
