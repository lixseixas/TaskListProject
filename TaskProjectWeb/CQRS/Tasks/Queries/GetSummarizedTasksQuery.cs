using MediatR;
using System;
using TaskProject.Models;

namespace TaskProject.CQRS.Tasks.Queries;

public class GetSummarizedTasksQuery : IRequest<SearchTaskModel>
{
    public DateTime InitialDate { get; set; }
    public DateTime FinalDate { get; set; }
}
