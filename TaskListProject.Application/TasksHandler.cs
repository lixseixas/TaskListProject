using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application
{
    /// <summary>
    /// Application service for handling task operations
    /// </summary>
    public class TasksHandler
    {
        private readonly TasksQueries tasksDal;
        public TasksHandler(TasksQueries tasksDal)
        {
            this.tasksDal = tasksDal ?? throw new ArgumentNullException(nameof(tasksDal));
        }

        /// <summary>
        /// Adds a task
        /// </summary>
        public bool AddTask(TaskDto taskDto)
        {
            var retorno = tasksDal.AddTask(taskDto);
            return retorno;
        }

        /// <summary>
        /// Gets tasks and returns as TaskDto list
        /// </summary>
        public bool GetTasks(ref List<TaskDto> taskList) => tasksDal.GetTasks(ref taskList);

        /// <summary>
        /// Validates task superposition
        /// </summary>
        public bool ValidateTaskSuperposition(Guid idAgendamento, DateTime data, DateTime dataInicial, DateTime dataFinal)
        {
            bool retorno = tasksDal.ValidateTaskSuperposition(idAgendamento, data, dataInicial, dataFinal);
            return retorno;
        }
    }
}
