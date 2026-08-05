using System.Collections.Generic;

namespace TaskReportApi.Models;

public class TaskListModel
{
    public List<TaskModel> TaskList { get; set; } = new();
}
