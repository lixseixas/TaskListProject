using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TaskProject.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskListProject.Infrastructure.Data
{
    public class TasksDal
    {
        private readonly TaskContext _context;

        // Keep parameterless constructor for tests or callers that rely on it.
        public TasksDal()
        {
            _context = GetContext();
        }

        // Constructor for dependency injection - allows the DbContext to be provided by the DI container.
        public TasksDal(TaskContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


       protected TaskContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
            var dbLocation = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["LocalDbConnection"];

            optionsBuilder.UseSqlServer(dbLocation);
            return new TaskContext(optionsBuilder.Options);
        }

        public bool AddTask( TaskDto taskDto)
        {
            try
            {
                if (taskDto.Inclusion == "edit")
                {
                    TaskDto obtainedTaskDto = new TaskDto();

                    GetTask(taskDto.Id, ref obtainedTaskDto);     
                    obtainedTaskDto.Description = taskDto.Description;
                    obtainedTaskDto.Date = taskDto.Date;
                    obtainedTaskDto.Title = taskDto.Title;
                    obtainedTaskDto.InitialHour = taskDto.InitialHour;
                    obtainedTaskDto.FinalHour = taskDto.FinalHour;
                    obtainedTaskDto.Priority = taskDto.Priority;
                    obtainedTaskDto.Ended = taskDto.Ended;
                    _context.SaveChanges();
                }
                else
                {
                    _context.Tasks.Add(taskDto);
                    _context.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }           

        }

        public bool GetTasks(ref List<TaskDto> taskList)
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

        public bool GetSummarizedTasks(DateTime dataInicial, DateTime dataFinal, ref List<SummarizedTasksDto> consolidatedList)
        {
            try
            {
                dataFinal = dataFinal.AddHours(23).AddMinutes(59);

                List<TaskDto> taskList = _context.Tasks.Where(p => p.Date >= dataInicial
                                                                   && p.Date <= dataFinal)
                                                                .OrderBy(p => p.Date)
                                                                .ToList();

                //daily total hours
                var summarizedListPerDay = taskList.GroupBy(p => p.Date).ToList();

                foreach (var item in summarizedListPerDay)
                {
                    SummarizedTasksDto summarized = new SummarizedTasksDto();

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

        public bool GetTask(Guid id, ref TaskDto taskDto)
        {
            var taskList = _context.Tasks.Where(p => p.Id == id).ToList();

            if (taskList != null && taskList.Count > 0)
            {
               
                var found = taskList.FirstOrDefault();
                if (found != null)
                {
                    taskDto.Id = found.Id;
                    taskDto.Title = found.Title;
                    taskDto.Description = found.Description;
                    taskDto.Date = found.Date;
                    taskDto.InitialHour = found.InitialHour;
                    taskDto.FinalHour = found.FinalHour;
                    taskDto.Priority = found.Priority;
                    taskDto.PriorityName = found.PriorityName;
                    taskDto.Ended = found.Ended;
                    taskDto.Inclusion = found.Inclusion;
                }
                return true;
            }
            else
            {
                return false;
            }         

        }

        // Changed: returns JWT token string when credentials are valid; null otherwise.
        public string GetUserPassword(string userLogin, string password)
        {
            var user = _context.UserLogins.Where(p => p.User == userLogin).FirstOrDefault();

            if (user == null)
            {
                return null;
            }

            // Plain-text comparison (existing behavior). Replace with hashed compare in production.
            if (user.Password != password)
            {
                return null;
            }

            // Read JWT settings from appsettings.json
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var jwtSection = config.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException("JWT configuration (Jwt:Key / Issuer / Audience) missing in appsettings.json");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.User),
                new Claim("UserId", user.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }

        public bool ValidateTaskSuperposition(Guid idAgendamento, DateTime data, DateTime dataInicial, DateTime dataFinal)
        {
            TaskDto itemFound = new TaskDto();
            
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
