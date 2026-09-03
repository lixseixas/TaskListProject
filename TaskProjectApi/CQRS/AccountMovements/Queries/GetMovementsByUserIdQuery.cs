using MediatR;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class GetMovementsByUserIdQuery : IRequest<AccountMovementsResponseModel>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
