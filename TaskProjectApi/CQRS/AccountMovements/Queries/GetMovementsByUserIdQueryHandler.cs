using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;
using Mapster;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class GetMovementsByUserIdQueryHandler : IRequestHandler<GetMovementsByUserIdQuery, AccountMovementsResponseModel>
{
    private readonly TaskContext _context;
    private readonly ILogger<GetMovementsByUserIdQueryHandler> _logger;

    public GetMovementsByUserIdQueryHandler(TaskContext context, ILogger<GetMovementsByUserIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AccountMovementsResponseModel> Handle(GetMovementsByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.AccountMovements
                .Where(m => m.UserId == request.UserId)
                .OrderByDescending(m => m.Date);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var movements = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var movementModels = movements.Adapt<List<AccountMovementModel>>();

            return new AccountMovementsResponseModel
            {
                Movements = movementModels,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements for UserId {UserId}", request.UserId);
            return new AccountMovementsResponseModel();
        }
    }
}
