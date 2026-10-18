using System.Collections.Generic;

namespace TaskReportApi.Models;

/// <summary>Represents the task list returned by the API.</summary>
public sealed record TaskListModel
{
    public List<TaskModel> TaskList { get; init; } = new();
}
