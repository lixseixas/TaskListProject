namespace TaskListProject.Application.Tasks;

public sealed record TaskResult
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public DateTime InitialHour { get; init; }
    public DateTime FinalHour { get; init; }
    public int Priority { get; init; }
    public bool Ended { get; init; }
}
