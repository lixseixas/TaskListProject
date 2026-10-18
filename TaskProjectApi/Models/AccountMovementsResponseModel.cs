using System;

namespace TaskReportApi.Models;

/// <summary>Represents a page of account movements with pagination metadata.</summary>
public sealed record AccountMovementsResponseModel
{
    public List<AccountMovementModel> Movements { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
