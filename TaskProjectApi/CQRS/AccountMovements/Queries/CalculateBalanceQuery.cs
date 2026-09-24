using MediatR;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.AccountMovements.Queries;

public class CalculateBalanceQuery : IRequest<AccountBalanceModel>
{
    public Guid UserId { get; set; }
}
