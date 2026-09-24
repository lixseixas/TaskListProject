using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskReportApi.Models;

[Table("AccountMovements")]
public class AccountMovementModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    public string? Description { get; set; }
}
