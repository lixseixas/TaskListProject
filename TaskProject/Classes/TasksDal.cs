using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Models;
using Microsoft.Extensions.Configuration;

namespace TaskProject.Bl
{
    public class TasksDal
    {
        private readonly TaskContext _context;

        public TasksDal()
        {
            _context = GetContext();
        }


       protected TaskContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            var dbLocation = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["LocalDbConnection"];

            optionsBuilder.UseSqlServer(dbLocation);
            return new TaskContext(optionsBuilder.Options);
        }

        public bool AddTask( TaskModel taskModel)
        {
            try
            {
                if (taskModel.Inclusion == "edit")
                {
                    TaskModel obtainedTaskModel = new TaskModel();

                    GetTask(taskModel.Id, ref obtainedTaskModel);

                    obtainedTaskModel.Description = taskModel.Description;
                    obtainedTaskModel.Date = taskModel.Date;
                    obtainedTaskModel.Title = taskModel.Title;
                    obtainedTaskModel.InitialHour = taskModel.InitialHour;
                    obtainedTaskModel.FinalHour = taskModel.FinalHour;
                    obtainedTaskModel.Priority = taskModel.Priority;
                    obtainedTaskModel.Ended = taskModel.Ended;

                    _context.SaveChanges();
                }
                else
                {
                    _context.Tasks.Add(taskModel);
                    _context.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }           

        }

        public bool GetTasks(ref List<TaskModel> taskList)
        {
            try
            {
                taskList = _context.Tasks.OrderBy(p => p.Date).ToList();

                foreach (var item in taskList)
                {
                    switch (item.Priority)
                    {
                        case 3:
                            item.PriorityName = "High";
                            break;
                        case 2:
                            item.PriorityName = "Medium";
                            break;
                        case 1:
                            item.PriorityName = "Low";
                            break;

                        default:
                            break;
                    }
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
                        

        }

        public bool GetSummarizedTasks(DateTime dataInicial, DateTime dataFinal, ref List<SummarizedTasksModel> consolidatedList)
        {
            try
            {
                dataFinal = dataFinal.AddHours(23).AddMinutes(59);

                List<TaskModel> taskList = _context.Tasks.Where(p => p.Date >= dataInicial
                                                                   && p.Date <= dataFinal)
                                                                .OrderBy(p => p.Date)
                                                                .ToList();

                //daily total hours
                var summarizedListPerDay = taskList.GroupBy(p => p.Date).ToList();

                foreach (var item in summarizedListPerDay)
                {
                    SummarizedTasksModel summarized = new SummarizedTasksModel();

                    TimeSpan totalHours = new TimeSpan();
                    TimeSpan totalHoursConcluded = new TimeSpan();

                    foreach (var tarefa in item)
                    {
                        TimeSpan hoursTarefa = DateTime.Parse(tarefa.FinalHour.ToShortTimeString()).Subtract(DateTime.Parse(tarefa.InitialHour.ToShortTimeString()));
                        totalHours = totalHours + hoursTarefa;

                        if (tarefa.Ended == true)
                        {
                            totalHoursConcluded = totalHoursConcluded + hoursTarefa;
                        }
                    }

                    summarized.Date = item.FirstOrDefault().Date;
                    summarized.Hours = totalHours.ToString();
                    summarized.TotalTasks = item.Count();
                    summarized.AverageHours = Convert.ToString(totalHours / item.Count());

                    //calculating percentual
                    double minutesTotal = totalHours.TotalMinutes;
                    double minutesConcluded = totalHoursConcluded.TotalMinutes;
                    double diference = minutesConcluded / minutesTotal;

                    summarized.PercentualConcludedTasks = diference = Math.Round(diference * 100);

                    consolidatedList.Add(summarized);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }


        public bool GetTask(Guid id, ref TaskModel taskModel)
        {
            var taskList = _context.Tasks.Where(p => p.Id == id).ToList();

            if (taskList.Count > 0)
            {
                taskModel = taskList.FirstOrDefault();
                return true;
            }

            else
            {
                return false;
            }         

        }

        public bool GetUserPassword(string userLogin, ref UserLoginModel userModel)
        {
            var userList = _context.UserLogins.Where(p => p.User == userLogin).ToList();

            if (userList.Count > 0)
            {
                userModel = userList.FirstOrDefault();
                return true;
            }

            else
            {
                return false;
            }


        }

        public bool ValidateTaskSuperposition(Guid idAgendamento, DateTime data, DateTime dataInicial, DateTime dataFinal)
        {
            TaskModel itemFound = new TaskModel();
            
            //filter all of tasks with the same date and diferente id
            var listaFiltrada = _context.Tasks.Where(p => p.Date >= data && p.Id != idAgendamento)
                                                                .OrderBy(p => p.Date).ToList();
                       
            if (listaFiltrada == null) 
            {
                return true;
            }

            // find task with initial hour
            itemFound = listaFiltrada.Where(p => p.InitialHour <= dataInicial
                                 && p.FinalHour >= dataInicial).FirstOrDefault();

            if (itemFound != null) 
            {               
                return false;
            }
            else
            {
                // find task with final hour
                itemFound = listaFiltrada.Where(p => p.InitialHour <= dataFinal
                                 && p.FinalHour >= dataFinal).FirstOrDefault();
            }

            if (itemFound != null) 
            {                
                return false;
            }
            else
            {
                // find task in the database
                itemFound = listaFiltrada.Where(p => p.InitialHour <= dataInicial
                    && p.FinalHour >= dataFinal ).FirstOrDefault();
            }
            if (itemFound != null)  
            {               
                return false;
            }
            else
            {
                // find task between other test
                itemFound = listaFiltrada.Where(p => p.InitialHour >= dataInicial
                    && p.FinalHour <= dataFinal ).FirstOrDefault();

            }
            if (itemFound != null)  
            {               
                return false;
            }

            return true;

        }

    }
}

