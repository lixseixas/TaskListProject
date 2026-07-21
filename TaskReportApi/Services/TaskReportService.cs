using Microsoft.EntityFrameworkCore;
using TaskReportApi.Data;
using TaskReportApi.Models;
using System.Globalization;

namespace TaskReportApi.Services;

/// <summary>
/// Service for generating task reports
/// </summary>
public class TaskReportService
{
    private readonly TaskContext _context;

    public TaskReportService(TaskContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets weekly task report for a specified date range
    /// </summary>
    /// <param name="startDate">Start date of the report range</param>
    /// <param name="endDate">End date of the report range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of weekly task reports</returns>
    public async Task<List<WeeklyTaskReportModel>> GetWeeklyTaskReportAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Normalize dates to start of day
        var normalizedStartDate = startDate.Date;
        var normalizedEndDate = endDate.Date.AddDays(1).AddTicks(-1);

        // Get tasks within the date range
        var tasks = await _context.WeeklyTasks
            .Where(t => t.WeekStartDate >= normalizedStartDate && t.WeekEndDate <= normalizedEndDate)
            .OrderBy(t => t.WeekStartDate)
            .ToListAsync(cancellationToken);             
                  

        return tasks;
    }

    /// <summary>
    /// Inserts a weekly task report entry.
    /// </summary>
    /// <param name="report">Weekly report entry to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The inserted report entry</returns>
    public async Task<WeeklyTaskReportModel> InsertWeeklyTaskReportAsync(
        WeeklyTaskReportModel report,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.WeekStartDate.Date > report.WeekEndDate.Date)
        {
            throw new ArgumentException("Week start date must be before or equal to week end date");
        }

        if (report.TotalTasks < 0 || report.CompletedTasks < 0 || report.PendingTasks < 0)
        {
            throw new ArgumentException("Task counts cannot be negative");
        }

        if (report.CompletedTasks + report.PendingTasks > report.TotalTasks)
        {
            throw new ArgumentException("Completed and pending tasks cannot exceed total tasks");
        }

        if (report.Id == Guid.Empty)
        {
            report.Id = Guid.NewGuid();
        }

        _context.WeeklyTasks.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return report;
    }
       
}
