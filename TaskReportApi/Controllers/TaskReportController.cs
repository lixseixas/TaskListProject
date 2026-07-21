using Microsoft.AspNetCore.Mvc;
using TaskReportApi.Models;
using TaskReportApi.Services;

namespace TaskReportApi.Controllers;

/// <summary>
/// API controller for task reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TaskReportController : ControllerBase
{
    private readonly TaskReportService _reportService;
    private readonly ILogger<TaskReportController> _logger;

    public TaskReportController(TaskReportService reportService, ILogger<TaskReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Gets weekly task report for a specified date range
    /// </summary>
    /// <param name="startDate">Start date of the report range (format: yyyy-MM-dd)</param>
    /// <param name="endDate">End date of the report range (format: yyyy-MM-dd)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of weekly task reports</returns>
    [HttpGet("weekly")]
    [ProducesResponseType(typeof(List<WeeklyTaskReportModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WeeklyTaskReportModel>>> GetWeeklyReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate date range
            if (startDate > endDate)
            {
                _logger.LogWarning("Invalid date range: StartDate {StartDate} is after EndDate {EndDate}", startDate, endDate);
                return BadRequest("Start date must be before or equal to end date");
            }

            // Limit date range to prevent excessive queries (max 3 year)
            if ((endDate - startDate).TotalDays > 1200)
            {
                _logger.LogWarning("Date range too large: {Days} days", (endDate - startDate).TotalDays);
                return BadRequest("Date range cannot exceed 1 year");
            }

            _logger.LogInformation("Generating weekly task report from {StartDate} to {EndDate}", startDate, endDate);

            var report = await _reportService.GetWeeklyTaskReportAsync(startDate, endDate, cancellationToken);

            _logger.LogInformation("Generated {Count} weekly report entries", report.Count);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly task report");
            return StatusCode(500, "An error occurred while generating the report");
        }
    }

    /// <summary>
    /// Inserts a weekly task report entry.
    /// </summary>
    /// <param name="report">Weekly report entry to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The inserted weekly task report entry</returns>
    [HttpPost("weekly")]
    [ProducesResponseType(typeof(WeeklyTaskReportModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeeklyTaskReportModel>> InsertWeeklyReport(
        [FromBody] WeeklyTaskReportModel report,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var insertedReport = await _reportService.InsertWeeklyTaskReportAsync(report, cancellationToken);

            return CreatedAtAction(
                nameof(GetWeeklyReport),
                new
                {
                    startDate = insertedReport.WeekStartDate,
                    endDate = insertedReport.WeekEndDate
                },
                insertedReport);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid weekly task report received");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting weekly task report");
            return StatusCode(500, "An error occurred while inserting the report");
        }
    }
}
