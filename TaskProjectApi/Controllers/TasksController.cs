using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskListProject.Application.Tasks.Commands;
using TaskListProject.Application.Tasks.Queries;
using TaskReportApi.Models;

namespace TaskReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TaskListModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskListModel>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await sender.Send(new GetTasksQuery(), cancellationToken);
        return Ok(new TaskListModel { TaskList = tasks.Adapt<List<TaskModel>>() });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await sender.Send(new GetTaskByIdQuery(id), cancellationToken);
        return task is null ? NotFound() : Ok(task.Adapt<TaskModel>());
    }

    [HttpPost("include")]
    [ProducesResponseType(typeof(TaskModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskModel>> Create([FromBody] TaskModel model, CancellationToken cancellationToken)
    {
        var task = await sender.Send(model.Adapt<CreateTaskCommand>(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task.Adapt<TaskModel>());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TaskModel model, CancellationToken cancellationToken)
    {
        var command = model.Adapt<UpdateTaskCommand>() with { Id = id };
        var updated = await sender.Send(command, cancellationToken);
        return updated is null ? NotFound() : NoContent();
    }

    [HttpGet("validate-superposition")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidateSuperposition(
        [FromQuery] Guid id, [FromQuery] DateTime date, [FromQuery] DateTime initial,
        [FromQuery] DateTime final, CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(new ValidateTaskSuperpositionQuery(id, date, initial, final), cancellationToken));
    }

    [HttpGet("summarized")]
    [ProducesResponseType(typeof(SearchTaskModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchTaskModel>> GetSummarized(
        [FromQuery] DateTime initialDate, [FromQuery] DateTime finalDate, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSummarizedTasksQuery { InitialDate = initialDate, FinalDate = finalDate }, cancellationToken);
        return Ok(result.Adapt<SearchTaskModel>());
    }
}
