using MediatR;
using TaskProject.Models;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetTasksQuery : IRequest<TaskListModel>
{
}
