using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaskProject.CQRS.Tasks.Commands;
using TaskProject.CQRS.Tasks.Queries;
using TaskProject.Models;
using TaskProject.Services;

namespace TaskProject.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
    private readonly ILogger<TaskController> _logger;
    private readonly IMediator _mediator;
    private readonly IWeeklyTaskReportPublisher _weeklyTaskReportPublisher;
    private readonly ITaskReportApiClient _apiClient;

        public TaskController(
            ILogger<TaskController> logger,
            IMediator mediator,
            IWeeklyTaskReportPublisher weeklyTaskReportPublisher,
            ITaskReportApiClient apiClient)
        {
            _logger = logger;
            _mediator = mediator;
            _weeklyTaskReportPublisher = weeklyTaskReportPublisher;
            _apiClient = apiClient;
        }

        public async Task<IActionResult> List()
        {
            try
            {
                var taskListModel = await _apiClient.GetTasksAsync();
                return View("List", taskListModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the task list");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> List(TaskListModel modelList)
        {
            if (!ModelState.IsValid)
            {
                return View(modelList);
            }

            return await List();
        }

        public IActionResult ListHoursPerDay()
        {
            SearchTaskModel pesquisaModel = new SearchTaskModel();
            pesquisaModel.ListTasksSummarized = new List<SummarizedTasksModel>();
            pesquisaModel.InitialDate = DateTime.Now.AddDays(-7);
            pesquisaModel.FinalDate = DateTime.Now.AddDays(7);

            return View(pesquisaModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListHoursPerDay(SearchTaskModel taskModel)
        {
            var query = new GetSummarizedTasksQuery
            {
                InitialDate = taskModel.InitialDate,
                FinalDate = taskModel.FinalDate
            };

            // Summarized tasks still powered by internal query handlers
            var result = await _mediator.Send(query);
            return Json(result);
        }

        public IActionResult Include()
        {
            TaskModel taskModel = new TaskModel();
            taskModel.Id = Guid.NewGuid();
            taskModel.Date = DateTime.Now;
            taskModel.Priority = 1;
            return View(taskModel);
        }

        [HttpPost]
        public async Task<IActionResult> Include(TaskModel taskModel)
        {
            if (!ModelState.IsValid)
            {
                return View(taskModel);
            }

            // create via TaskReportApi
            var created = await _apiClient.CreateTaskAsync(taskModel);
            if (!created)
            {
                return View("Error");
            }

            return await List();
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var taskModel = await _apiClient.GetTaskByIdAsync(id);

            if (taskModel == null || taskModel.Id == Guid.Empty)
            {
                return View("Error");
            }

            taskModel.Inclusion = "edit";
            return View("Include", taskModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TaskModel taskModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Include", taskModel);
            }

            var updated = await _apiClient.UpdateTaskAsync(taskModel.Id, taskModel);
            if (!updated)
            {
                return View("Error");
            }

            return await List();
        }

        public IActionResult TestAspNetFunctions()
        {
            TestAspNetFunctionsModel taskModel = new TestAspNetFunctionsModel();
            return View(taskModel);
        }

        [HttpPost]
        public IActionResult TestAspNetFunctions(TestAspNetFunctionsModel taskModel)
        {
            if (!ModelState.IsValid)
            {
                return View(taskModel);
            }

            if (!string.IsNullOrWhiteSpace(taskModel.InputA) && !string.IsNullOrWhiteSpace(taskModel.InputB))
            {
                taskModel.OutPut = taskModel.InputA + taskModel.InputB;
                return View("TestAspNetFunctions", taskModel);
            }

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendWeeklyTaskReport(CancellationToken cancellationToken)
        {
            var today = DateTime.Today;
            var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            var weekStart = today.AddDays(-daysSinceMonday);
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;

            var report = new WeeklyTaskReportModel
            {
                Id = Guid.NewGuid(),
                WeekStartDate = weekStart,
                WeekEndDate = weekStart.AddDays(6),
                WeekNumber = calendar.GetWeekOfYear(weekStart, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                Year = weekStart.Year,
                TotalTasks = 0,
                CompletedTasks = 0,
                PendingTasks = 0,
                CompletionPercentage = 0
            };

            try
            {
                await _weeklyTaskReportPublisher.PublishAsync(report, cancellationToken);
                TempData["RabbitMqSuccess"] = "Weekly task report sent to RabbitMQ.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending the weekly task report to RabbitMQ.");
                TempData["RabbitMqError"] = "Unable to send the weekly task report to RabbitMQ.";
            }

            return RedirectToAction(nameof(TestAspNetFunctions));
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
