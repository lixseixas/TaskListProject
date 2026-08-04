using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskProject.Models;

namespace TaskProject.Services
{
    public class TaskReportApiClient : ITaskReportApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TaskReportApiClient> _logger;

        public TaskReportApiClient(HttpClient http, ILogger<TaskReportApiClient> logger)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger;
        }

        public async Task<TaskListModel> GetTasksAsync()
        {
            try
            {
                var resp = await _http.GetFromJsonAsync<TaskListModel>("api/tasks");
                return resp ?? new TaskListModel { TaskList = new List<TaskModel>() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi GetTasks");
                return new TaskListModel { TaskList = new List<TaskModel>() };
            }
        }

        public async Task<TaskModel> GetTaskByIdAsync(Guid id)
        {
            try
            {
                var resp = await _http.GetFromJsonAsync<TaskModel>($"api/tasks/{id}");
                return resp ?? new TaskModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi GetTaskById");
                return new TaskModel();
            }
        }

        public async Task<bool> CreateTaskAsync(TaskModel model)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/tasks", model);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi CreateTask");
                return false;
            }
        }

        public async Task<bool> UpdateTaskAsync(Guid id, TaskModel model)
        {
            try
            {
                var resp = await _http.PutAsJsonAsync($"api/tasks/{id}", model);
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi UpdateTask");
                return false;
            }
        }

        public async Task<List<SummarizedTasksModel>> GetSummarizedTasksAsync(DateTime initial, DateTime final)
        {
            try
            {
                // The API does not currently expose a summarized endpoint; fallback to empty list
                // Keep method for compatibility; if API adds endpoint, update the URL here.
                return new List<SummarizedTasksModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling TaskReportApi GetSummarizedTasks");
                return new List<SummarizedTasksModel>();
            }
        }
    }
}
