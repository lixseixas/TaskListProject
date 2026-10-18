using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TaskListProject.Infrastructure.Data;
using TaskListProject.Application.Tasks.Commands;
using TaskListProject.Application.Tasks.Queries;
using TaskProject.Domain.Entities;

namespace TestProject;

public class DbTests
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<TaskContext> _options = null!;

    [SetUp]
    public async Task Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();
        _options = new DbContextOptionsBuilder<TaskContext>().UseSqlite(_connection).Options;
        await using var context = new TaskContext(_options);
        await context.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task Cleanup() => await _connection.DisposeAsync();

    [Test]
    public async Task CreateAndUpdatePersistAllTaskFields()
    {
        var task = NewTask();
        await using (var context = new TaskContext(_options))
        {
            await new TasksCommands(context).AddAsync(task, CancellationToken.None);
        }

        var changed = NewTask();
        changed.Id = task.Id;
        changed.Title = "Changed title";
        changed.Description = "Changed description";
        changed.Date = task.Date.AddDays(1);
        changed.InitialHour = task.InitialHour.AddHours(1);
        changed.FinalHour = task.FinalHour.AddHours(2);
        changed.Priority = 3;
        changed.Ended = true;
        await using (var context = new TaskContext(_options))
        {
            Assert.That(await new TasksCommands(context).UpdateAsync(changed, CancellationToken.None), Is.Not.Null);
        }

        await using var verification = new TaskContext(_options);
        var saved = await new TasksQueries(verification).GetByIdAsync(task.Id, CancellationToken.None);
        Assert.That(saved, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(saved!.Title, Is.EqualTo(changed.Title));
            Assert.That(saved.Description, Is.EqualTo(changed.Description));
            Assert.That(saved.Date, Is.EqualTo(changed.Date));
            Assert.That(saved.InitialHour, Is.EqualTo(changed.InitialHour));
            Assert.That(saved.FinalHour, Is.EqualTo(changed.FinalHour));
            Assert.That(saved.Priority, Is.EqualTo(3));
            Assert.That(saved.Ended, Is.True);
        });
        Assert.That(await verification.Tasks.CountAsync(), Is.EqualTo(1));
        Assert.That(verification.ChangeTracker.Entries(), Is.Empty);
    }

    [Test]
    public async Task MissingTaskIsNotInsertedByUpdate()
    {
        await using var context = new TaskContext(_options);
        Assert.That(await new TasksCommands(context).UpdateAsync(NewTask(), CancellationToken.None), Is.Null);
        Assert.That(await new TasksQueries(context).GetByIdAsync(Guid.NewGuid(), CancellationToken.None), Is.Null);
        Assert.That(await context.Tasks.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task QueriesPreserveSummaryAndSuperpositionSemantics()
    {
        await using var context = new TaskContext(_options);
        var task = NewTask();
        task.Ended = true;
        await new TasksCommands(context).AddAsync(task, CancellationToken.None);
        var queries = new TasksQueries(context);
        var list = await queries.GetAllAsync(CancellationToken.None);
        Assert.That(list.Single().PriorityName, Is.EqualTo("Low"));
        var summary = (await queries.GetSummarizedAsync(task.Date, task.Date, CancellationToken.None)).Single();
        Assert.Multiple(() =>
        {
            Assert.That(summary.Hours, Is.EqualTo("01:00:00"));
            Assert.That(summary.AverageHours, Is.EqualTo("01:00:00"));
            Assert.That(summary.TotalTasks, Is.EqualTo(1));
            Assert.That(summary.PercentualConcludedTasks, Is.EqualTo(100));
        });
        Assert.That(await queries.IsTimeAvailableAsync(Guid.NewGuid(), task.Date,
            task.InitialHour, task.FinalHour, CancellationToken.None), Is.False);
        Assert.That(await queries.IsTimeAvailableAsync(task.Id, task.Date,
            task.InitialHour, task.FinalHour, CancellationToken.None), Is.True);
    }

    [Test]
    public void PersistenceMappingDoesNotIncludePresentationFields()
    {
        using var context = new TaskContext(_options);
        var entity = context.Model.FindEntityType(typeof(TaskDto))!;
        Assert.That(entity.GetTableName(), Is.EqualTo("Tasks"));
        Assert.That(entity.FindProperty("PriorityName"), Is.Null);
        Assert.That(entity.FindProperty("Inclusion"), Is.Null);
    }

    [Test]
    public async Task TaskReadsPropagateDatabaseFailuresAndCancellation()
    {
        await using var context = new TaskContext(_options);
        var queries = new TasksQueries(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(() => queries.GetAllAsync(cancellation.Token));
        await context.Database.ExecuteSqlRawAsync("DROP TABLE Tasks");
        Assert.ThrowsAsync<SqliteException>(() => queries.GetAllAsync(CancellationToken.None));
        Assert.ThrowsAsync<SqliteException>(() => queries.GetSummarizedAsync(
            DateTime.Today, DateTime.Today, CancellationToken.None));
    }

    [Test]
    public void TaskHandlersForwardCancellationToPersistence()
    {
        using var context = new TaskContext(_options);
        var queries = new TasksQueries(context);
        var commands = new TasksCommands(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var token = cancellation.Token;
        Assert.CatchAsync<OperationCanceledException>(() =>
            new GetTasksQueryHandler(queries).Handle(new GetTasksQuery(), token));
        Assert.CatchAsync<OperationCanceledException>(() =>
            new GetTaskByIdQueryHandler(queries).Handle(new GetTaskByIdQuery(Guid.NewGuid()), token));
        Assert.CatchAsync<OperationCanceledException>(() =>
            new GetSummarizedTasksQueryHandler(queries).Handle(new GetSummarizedTasksQuery
            {
                InitialDate = DateTime.Today, FinalDate = DateTime.Today
            }, token));
        Assert.CatchAsync<OperationCanceledException>(() =>
            new ValidateTaskSuperpositionQueryHandler(queries).Handle(
                new ValidateTaskSuperpositionQuery(Guid.NewGuid(), DateTime.Today, DateTime.Today, DateTime.Today), token));
        Assert.CatchAsync<OperationCanceledException>(() =>
            new CreateTaskCommandHandler(commands).Handle(new CreateTaskCommand
            {
                Title = "Task", Description = "Description"
            }, token));
        Assert.CatchAsync<OperationCanceledException>(() =>
            new UpdateTaskCommandHandler(commands).Handle(new UpdateTaskCommand { Id = Guid.NewGuid() }, token));
    }

    private static TaskDto NewTask() => new()
    {
        Id = Guid.NewGuid(), Title = "Task", Description = "Description",
        Date = new DateTime(2026, 10, 17), InitialHour = new DateTime(2026, 10, 17, 9, 0, 0),
        FinalHour = new DateTime(2026, 10, 17, 10, 0, 0), Priority = 1
    };
}
