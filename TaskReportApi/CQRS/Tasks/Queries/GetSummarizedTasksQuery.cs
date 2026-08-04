using MediatR;
using System;
using TaskReportApi.Models;

namespace TaskReportApi.CQRS.Tasks.Queries;

public class GetSummarizedTasksQuery : IRequest<SearchTaskModel>
{
    public DateTime InitialDate { get; set; }
    public DateTime FinalDate { get; set; }
}
