using TaskProject.Domain.Entities;

namespace TaskListProject.Application.Data;

public interface ITasksQueries
{
    bool AddTask(TaskDto taskDto);
    bool GetTasks(ref List<TaskDto> taskList);
    bool GetTask(Guid id, ref TaskDto taskDto);
    bool GetSummarizedTasks(DateTime dataInicial, DateTime dataFinal, ref List<SummarizedTasksDto> consolidatedList);
    bool ValidateTaskSuperposition(Guid idAgendamento, DateTime data, DateTime dataInicial, DateTime dataFinal);
}
