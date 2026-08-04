using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TaskReportApi.Models;
using TaskProject.Domain.Entities;
using TaskListProject.Application;
using TaskListProject.Infrastructure.Data;
using MediatR;
using TaskReportApi.CQRS.Tasks.Queries;

namespace TaskReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TasksHandler _tasksHandler;
    private readonly TasksQueries _tasksQueries;
    private readonly ILogger<TasksController> _logger;
    private readonly IMediator _mediator;

    public TasksController(
        TasksHandler tasksHandler,
        TasksQueries tasksQueries,
        ILogger<TasksController> logger,
        IMediator mediator)
    {
        _tasksHandler = tasksHandler;
        _tasksQueries = tasksQueries;
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TaskListModel), 200)]
    public ActionResult<TaskListModel> GetAll()
    {
        try
        {
            var list = new List<TaskDto>();
            var ok = _tasksHandler.GetTasks(ref list);

            if (!ok)
            {
                return StatusCode(500, "Error retrieving tasks");
            }

            var result = list.Select(d => new TaskModel
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                Date = d.Date,
                InitialHour = d.InitialHour,
                FinalHour = d.FinalHour,
                Priority = d.Priority,
                Ended = d.Ended
            }).ToList();

            return Ok(new TaskListModel { TaskList = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Get tasks");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskModel), 200)]
    [ProducesResponseType(404)]
    public ActionResult<TaskModel> GetById(Guid id)
    {
        try
        {
            var dto = new TaskDto();
            var found = _tasksQueries.GetTask(id, ref dto);

            if (!found)
                return NotFound();

            var model = new TaskModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                InitialHour = dto.InitialHour,
                FinalHour = dto.FinalHour,
                Priority = dto.Priority,
                Ended = dto.Ended
            };

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetById");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public ActionResult Create([FromBody] TaskModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var dto = new TaskDto
            {
                Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
                Title = model.Title,
                Description = model.Description,
                Date = model.Date,
                InitialHour = model.InitialHour,
                FinalHour = model.FinalHour,
                Priority = model.Priority,
                Ended = model.Ended,
                Inclusion = "new"
            };

            var ok = _tasksHandler.AddTask(dto);
            if (!ok) return StatusCode(500, "Error creating task");

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public ActionResult Update(Guid id, [FromBody] TaskModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var dto = new TaskDto
            {
                Id = id,
                Title = model.Title,
                Description = model.Description,
                Date = model.Date,
                InitialHour = model.InitialHour,
                FinalHour = model.FinalHour,
                Priority = model.Priority,
                Ended = model.Ended,
                Inclusion = "edit"
            };

            var ok = _tasksHandler.AddTask(dto);
            if (!ok) return StatusCode(500, "Error updating task");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpGet("validate-superposition")]
    public ActionResult<bool> ValidateSuperposition([FromQuery] Guid id, [FromQuery] DateTime date, [FromQuery] DateTime initial, [FromQuery] DateTime final)
    {
        try
        {
            var ok = _tasksHandler.ValidateTaskSuperposition(id, date, initial, final);
            return Ok(ok);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating superposition");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpGet("summarized")]
    [ProducesResponseType(typeof(SearchTaskModel), 200)]
    public async Task<ActionResult<SearchTaskModel>> GetSummarized([FromQuery] DateTime initialDate, [FromQuery] DateTime finalDate)
    {
        try
        {
            var query = new GetSummarizedTasksQuery
            {
                InitialDate = initialDate,
                FinalDate = finalDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving summarized tasks");
            return StatusCode(500, "An error occurred");
        }
    }
}
