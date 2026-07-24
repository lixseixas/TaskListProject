using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;

namespace TaskListProject.Application
{
    public class TasksHandler
    {
        private readonly TasksDal tasksDal;
        public TasksHandler(TasksDal tasksDal)
        {
            this.tasksDal = tasksDal ?? throw new ArgumentNullException(nameof(tasksDal));
        }

        public bool AddTask(TaskDto taskDto)
        {


            var retorno = tasksDal.AddTask(taskDto);

            return retorno;
        }

        public bool GetTasks(ref List<TaskDto> taskList) => tasksDal.GetTasks(ref taskList);

        public bool ValidateTaskSuperposition(Guid idAgendamento, DateTime data, DateTime dataInicial, DateTime dataFinal)
        {

            bool retorno = tasksDal.ValidateTaskSuperposition(idAgendamento, data, dataInicial, dataFinal);

            return retorno;
        }
    }
}
