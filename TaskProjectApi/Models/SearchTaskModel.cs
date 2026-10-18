using System;
using System.Collections.Generic;

namespace TaskReportApi.Models;

/// <summary>Represents task summaries for the requested date interval.</summary>
public sealed record SearchTaskModel
{
    public DateTime InitialDate { get; init; }
    public DateTime FinalDate { get; init; }
    public List<SummarizedTasksModel> ListTasksSummarized { get; init; } = new();
}
