using MediatR;
using TaskProject.Models;

namespace TaskProject.CQRS.Tasks.Commands;

public class UpdateTaskCommand : IRequest<bool>
{
    public TaskModel Task { get; set; } = new TaskModel();
}
