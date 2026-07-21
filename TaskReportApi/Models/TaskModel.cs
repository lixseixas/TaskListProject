using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskReportApi.Models;

/// <summary>
/// Represents a task in the system
/// </summary>
[Table("Tasks")]
public class TaskModel
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public DateTime InitialHour { get; set; }

    [Required]
    public DateTime FinalHour { get; set; }

    [Required]
    public int Priority { get; set; }

    [Required]
    public bool Ended { get; set; }
}
