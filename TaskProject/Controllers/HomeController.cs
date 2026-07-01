using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TaskProject.Bl;
using TaskProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace TaskProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TaskContext _context;

        public HomeController(ILogger<HomeController> logger, TaskContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            try
            {
                List<TaskModel> taskList = new List<TaskModel>();

                TasksDal taskDb = new TasksDal();
                bool retorno = taskDb.GetTasks(ref taskList);

                if (retorno == false)
                {
                    return View("Error");
                }

                TaskListModel taskListModel = new TaskListModel();
                taskListModel.TaskList = taskList;

                return View("List", taskListModel);
            }
            catch (Exception ex)
            {
                // Log the exception with the configured logging provider (log4net)
                _logger.LogError(ex, "An error occurred while retrieving the task list in HomeController.List.");

                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult List(TaskListModel modelList)
        {
            if (!ModelState.IsValid)
            {
                return View(modelList);
            }

            return List();
        }

        public IActionResult ListHoursPerDay()
        {
            List<SummarizedTasksModel> listaTasks = new List<SummarizedTasksModel>();

            SearchTaskModel pesquisaModel = new SearchTaskModel();
            pesquisaModel.ListTasksSummarized = listaTasks;
            pesquisaModel.InitialDate = DateTime.Now.AddDays(-7);
            pesquisaModel.FinalDate = DateTime.Now.AddDays(7);

            return View(pesquisaModel);
        }

        [HttpPost]
        public IActionResult ListHoursPerDay(SearchTaskModel taskModel)
        {
            List<SummarizedTasksModel> listaTasks = new List<SummarizedTasksModel>();
            TasksDal taskDb = new TasksDal();
            bool retorno = taskDb.GetSummarizedTasks(taskModel.InitialDate,
                                                        taskModel.FinalDate, ref listaTasks);

            if (retorno == false)
            {
                return View("Error");
            }

            taskModel.ListTasksSummarized = listaTasks;
            return Json(taskModel);
        }

        public IActionResult Include()
        {
            TaskModel taskModel = new TaskModel();
            taskModel.Id = Guid.NewGuid();
            taskModel.Date = DateTime.Now;
            taskModel.Priority = 1;
            return View(taskModel);
        }

        bool ValidarTask(TaskModel taskModel, ref string errorMessage)
        {
            if (taskModel.InitialHour == taskModel.FinalHour)
            {
                errorMessage = "The final hour and initial hour are the same.";
                return false;
            }

            DateTime dataInicioTask = taskModel.Date.AddHours(taskModel.InitialHour.Hour)
                .AddMinutes(taskModel.InitialHour.Minute);

            DateTime dataAgora = DateTime.Now;
            if (dataInicioTask < dataAgora)
            {
                errorMessage = "The initial hour is older than the current.";
                return false;
            }

            DateTime dataFimTask = taskModel.Date.AddHours(taskModel.FinalHour.Hour)
               .AddMinutes(taskModel.FinalHour.Minute);

            if (dataFimTask < dataAgora)
            {
                errorMessage = "The final date is older than the current.";
                return false;
            }

            if (dataFimTask < dataInicioTask)
            {
                errorMessage = "The final date must be higher than the current";
                return false;
            }

            TimeSpan hoursTask = DateTime.Parse(taskModel.FinalHour.ToShortTimeString()).Subtract(DateTime.Parse(taskModel.InitialHour.ToShortTimeString()));

            double minutosTotais = hoursTask.TotalMinutes;

            if (minutosTotais > 300)
            {
                errorMessage = "The duration of the task is larger than 5 hours";
                return false;
            }

            TasksDal taskDb = new TasksDal();
            bool retornoSobreposicao = taskDb.ValidateTaskSuperposition(taskModel.Id, taskModel.Date,
                                                                        dataInicioTask, dataFimTask);
            if (retornoSobreposicao == false)
            {
                errorMessage = "Superposition of task, please find another date";
                return false;
            }

            return true;
        }

        [HttpPost]
        public IActionResult Include(TaskModel taskModel)
        {
            if (!ModelState.IsValid)
            {
                return View(taskModel);
            }

            string errorMessage = "";

            if (!ValidarTask(taskModel, ref errorMessage))
            {
                ModelState.AddModelError("", errorMessage);
                return View(taskModel);
            }

            taskModel.Id = Guid.NewGuid();
            taskModel.InitialHour = taskModel.Date.AddHours(taskModel.InitialHour.Hour).AddMinutes(taskModel.InitialHour.Minute);
            taskModel.FinalHour = taskModel.Date.AddHours(taskModel.FinalHour.Hour).AddMinutes(taskModel.FinalHour.Minute);

            TasksDal taskDb = new TasksDal();
            bool retorno = taskDb.AddTask(taskModel);

            if (retorno == false)
            {
                return View("Error");
            }

            return List();
        }

        public IActionResult Edit(Guid id)
        {
            TaskModel taskModel = new TaskModel();

            TasksDal taskDb = new TasksDal();
            bool retorno = taskDb.GetTask(id, ref taskModel);

            if (retorno == false)
            {
                return View("Error");
            }

            return View("Include", taskModel);
        }

        [HttpPost]
        public IActionResult Edit(TaskModel taskModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Include", taskModel);
            }

            string errorMessage = "";

            if (!ValidarTask(taskModel, ref errorMessage))
            {
                ModelState.AddModelError("", errorMessage);
                return View("Include", taskModel);
            }

            taskModel.Inclusion = "edit";
            TasksDal taskDb = new TasksDal();

            taskModel.InitialHour = taskModel.Date.AddHours(taskModel.InitialHour.Hour).AddMinutes(taskModel.InitialHour.Minute);
            taskModel.FinalHour = taskModel.Date.AddHours(taskModel.FinalHour.Hour).AddMinutes(taskModel.FinalHour.Minute);

            bool retorno = taskDb.AddTask(taskModel);
            if (retorno == false)
            {
                return View("Error");
            }

            return List();
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

            return List();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
