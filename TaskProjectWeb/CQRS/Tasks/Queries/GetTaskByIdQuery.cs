using MediatR;
using System;
using TaskProject.Models;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetTaskByIdQuery : IRequest<TaskModel>
{
    public Guid Id { get; set; }
}
