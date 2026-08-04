using System;

namespace TaskReportApi.Models;

public class SummarizedTasksModel
{
    public DateTime Date { get; set; }
    public string Hours { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public string AverageHours { get; set; } = string.Empty;
    public double PercentualConcludedTasks { get; set; }
}
