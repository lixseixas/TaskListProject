using MediatR;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.AccountMovements.Commands;

public class CreateMovementCommand : IRequest<AccountMovementModel>
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
