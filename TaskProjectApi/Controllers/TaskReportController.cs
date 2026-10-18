using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskListProject.Application.WeeklyTaskReports.Commands;
using TaskListProject.Application.WeeklyTaskReports.Queries;
using TaskReportApi.Models;

namespace TaskReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskReportController(ISender sender) : ControllerBase
{
    [HttpGet("weekly")]
    [ProducesResponseType(typeof(List<WeeklyTaskReportModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WeeklyTaskReportModel>>> GetWeeklyReport(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken = default)
    {
        var reports = await sender.Send(new GetWeeklyTaskReportsQuery(startDate, endDate), cancellationToken);
        return Ok(reports.Adapt<List<WeeklyTaskReportModel>>());
    }

    [HttpPost("weekly")]
    [ProducesResponseType(typeof(WeeklyTaskReportModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeeklyTaskReportModel>> InsertWeeklyReport(
        [FromBody] WeeklyTaskReportModel report, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(report.Adapt<InsertWeeklyTaskReportCommand>(), cancellationToken);
        return CreatedAtAction(nameof(GetWeeklyReport),
            new { startDate = result.WeekStartDate, endDate = result.WeekEndDate },
            result.Adapt<WeeklyTaskReportModel>());
    }
}
