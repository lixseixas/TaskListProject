using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskProject.Models;

namespace TaskProject.Services
{
    public interface ITaskReportApiClient
    {
        Task<TaskListModel> GetTasksAsync();
        Task<TaskModel> GetTaskByIdAsync(Guid id);
        Task<bool> CreateTaskAsync(TaskModel model);
        Task<bool> UpdateTaskAsync(Guid id, TaskModel model);
        Task<List<SummarizedTasksModel>> GetSummarizedTasksAsync(DateTime initial, DateTime final);
    }
}
