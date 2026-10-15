using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TaskListProject.Application;
using TaskListProject.Application.Data;
using TaskListProject.Infrastructure;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.CQRS.AccountMovements.Commands;
using TaskReportApi.CQRS.AccountMovements.Queries;
using TaskReportApi.Models;
using TaskReportApi.Services;

namespace TestProject;

public class DataAccessBoundaryTests
{
    [Test]
    public void InfrastructureRegistersScopedRepositories()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:LocalDbConnection"] = "Server=(localdb)\\mssqllocaldb;Database=BoundaryTest;Trusted_Connection=True"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.AddScoped<LoginHandler>();
        services.AddScoped<TasksHandler>();
        services.AddScoped<TaskReportService>();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();
        var first = scope.ServiceProvider.GetRequiredService<IWeeklyTaskReportRepository>();
        Assert.That(first, Is.TypeOf<WeeklyTaskReportRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IWeeklyTaskReportRepository>(), Is.SameAs(first));
        Assert.That(scope.ServiceProvider.GetRequiredService<IAccountMovementRepository>(), Is.TypeOf<AccountMovementRepository>());
        Assert.That(scope.ServiceProvider.GetRequiredService<ITasksQueries>(), Is.TypeOf<TasksQueries>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IUserQueries>(), Is.TypeOf<UserQueries>());
        Assert.That(scope.ServiceProvider.GetRequiredService<LoginHandler>(), Is.Not.Null);
        Assert.That(scope.ServiceProvider.GetRequiredService<TasksHandler>(), Is.Not.Null);
        Assert.That(scope.ServiceProvider.GetRequiredService<TaskReportService>(), Is.Not.Null);
        using var otherScope = provider.CreateScope();
        Assert.That(otherScope.ServiceProvider.GetRequiredService<IWeeklyTaskReportRepository>(), Is.Not.SameAs(first));
    }

    [Test]
    public void MissingConnectionStringFailsExplicitly()
    {
        var configuration = new ConfigurationBuilder().Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddInfrastructure(configuration));
    }

    [Test]
    public void ApplicationHasNoInfrastructureOrEfAssemblyReferences()
    {
        var references = typeof(TasksHandler).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        Assert.That(references, Has.None.EqualTo("TaskListProject.InfraStructure"));
        Assert.That(references.Any(n => n != null && n.StartsWith("Microsoft.EntityFrameworkCore")), Is.False);
    }

    [Test]
    public async Task WeeklyReportsNormalizeDatesAndMapResults()
    {
        var repository = new ReportRepositoryStub();
        var report = new WeeklyTaskReportDto
        {
            Id = Guid.NewGuid(), WeekStartDate = new DateTime(2026, 10, 5),
            WeekEndDate = new DateTime(2026, 10, 11), WeekNumber = 41, Year = 2026,
            TotalTasks = 10, CompletedTasks = 8, PendingTasks = 2, CompletionPercentage = 80
        };
        repository.Reports.Add(report);
        using var cancellation = new CancellationTokenSource();
        var service = new TaskReportService(repository);
        var results = await service.GetWeeklyTaskReportAsync(
            report.WeekStartDate.AddHours(12), report.WeekEndDate.AddHours(12), cancellation.Token);
        Assert.That(repository.StartDate, Is.EqualTo(report.WeekStartDate));
        Assert.That(repository.EndDate, Is.EqualTo(report.WeekEndDate.AddDays(1).AddTicks(-1)));
        Assert.That(repository.Token, Is.EqualTo(cancellation.Token));
        Assert.That(results.Single().Id, Is.EqualTo(report.Id));
        Assert.That(results.Single().TotalTasks, Is.EqualTo(10));
        Assert.That(results.Single().CompletionPercentage, Is.EqualTo(80));
    }

    [Test]
    public async Task WeeklyReportInsertGeneratesIdAndForwardsCancellation()
    {
        var repository = new ReportRepositoryStub();
        using var cancellation = new CancellationTokenSource();
        var service = new TaskReportService(repository);
        var result = await service.InsertWeeklyTaskReportAsync(new WeeklyTaskReportModel
        {
            WeekStartDate = new DateTime(2026, 10, 5), WeekEndDate = new DateTime(2026, 10, 11),
            TotalTasks = 10, CompletedTasks = 8, PendingTasks = 2
        }, cancellation.Token);
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(repository.Reports.Single().Id, Is.EqualTo(result.Id));
        Assert.That(repository.Token, Is.EqualTo(cancellation.Token));
    }

    [Test]
    public void InvalidWeeklyReportIsNotPersisted()
    {
        var repository = new ReportRepositoryStub();
        var service = new TaskReportService(repository);
        Assert.ThrowsAsync<ArgumentException>(() => service.InsertWeeklyTaskReportAsync(new WeeklyTaskReportModel
        {
            TotalTasks = 1, CompletedTasks = 2
        }));
        Assert.That(repository.Reports, Is.Empty);
    }

    [Test]
    public async Task MovementCreationPersistsAndMapsTheMovement()
    {
        var repository = new MovementRepositoryStub { Balance = 10 };
        var handler = new CreateMovementCommandHandler(repository, NullLogger<CreateMovementCommandHandler>.Instance);
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var result = await handler.Handle(new CreateMovementCommand
        {
            UserId = userId, Amount = -5, Type = "Debit", Date = new DateTime(2026, 10, 14), Description = "Test"
        }, cancellation.Token);
        Assert.That(repository.Added, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(repository.Added!.Id));
        Assert.That(result.Amount, Is.EqualTo(-5));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(repository.Token, Is.EqualTo(cancellation.Token));
    }

    [Test]
    public void NegativeBalancePreventsMovementPersistence()
    {
        var repository = new MovementRepositoryStub { Balance = 10 };
        var handler = new CreateMovementCommandHandler(repository, NullLogger<CreateMovementCommandHandler>.Instance);
        Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateMovementCommand { UserId = Guid.NewGuid(), Amount = -11 }, CancellationToken.None));
        Assert.That(repository.Added, Is.Null);
    }

    [TestCase(10, -3, 7)]
    [TestCase(2, -3, 0)]
    public async Task BalancePreservesCalculationAndZeroFloor(int credit, int debit, int expected)
    {
        var repository = new MovementRepositoryStub();
        repository.Movements.Add(new AccountMovementDto { Amount = credit });
        repository.Movements.Add(new AccountMovementDto { Amount = debit });
        var handler = new CalculateBalanceQueryHandler(repository, NullLogger<CalculateBalanceQueryHandler>.Instance);
        var userId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var result = await handler.Handle(new CalculateBalanceQuery { UserId = userId }, cancellation.Token);
        Assert.That(result.Balance, Is.EqualTo(expected));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(repository.UserId, Is.EqualTo(userId));
        Assert.That(repository.Token, Is.EqualTo(cancellation.Token));
    }

    [Test]
    public async Task MovementPagePreservesMetadataAndMapping()
    {
        var repository = new MovementRepositoryStub { TotalCount = 50 };
        repository.Movements.Add(new AccountMovementDto { Id = Guid.NewGuid(), Amount = 42 });
        var handler = new GetMovementsByUserIdQueryHandler(repository, NullLogger<GetMovementsByUserIdQueryHandler>.Instance);
        var userId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var result = await handler.Handle(new GetMovementsByUserIdQuery
        {
            UserId = userId, Page = 2, PageSize = 10
        }, cancellation.Token);
        Assert.That(result.TotalCount, Is.EqualTo(50));
        Assert.That(result.Page, Is.EqualTo(2));
        Assert.That(result.PageSize, Is.EqualTo(10));
        Assert.That(result.Movements.Single().Id, Is.EqualTo(repository.Movements.Single().Id));
        Assert.That(repository.UserId, Is.EqualTo(userId));
        Assert.That(repository.Page, Is.EqualTo(2));
        Assert.That(repository.PageSize, Is.EqualTo(10));
        Assert.That(repository.Token, Is.EqualTo(cancellation.Token));
    }

    [Test]
    public void QueryFailuresAreNotSuccessfulEmptyResponses()
    {
        var repository = new MovementRepositoryStub { Failure = new InvalidOperationException("Database unavailable") };
        var balanceHandler = new CalculateBalanceQueryHandler(repository, NullLogger<CalculateBalanceQueryHandler>.Instance);
        var pageHandler = new GetMovementsByUserIdQueryHandler(repository, NullLogger<GetMovementsByUserIdQueryHandler>.Instance);
        Assert.ThrowsAsync<InvalidOperationException>(() => balanceHandler.Handle(
            new CalculateBalanceQuery(), CancellationToken.None));
        Assert.ThrowsAsync<InvalidOperationException>(() => pageHandler.Handle(
            new GetMovementsByUserIdQuery(), CancellationToken.None));
    }

    private sealed class ReportRepositoryStub : IWeeklyTaskReportRepository
    {
        public List<WeeklyTaskReportDto> Reports { get; } = [];
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public CancellationToken Token { get; private set; }

        public Task<List<WeeklyTaskReportDto>> GetAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            StartDate = startDate;
            EndDate = endDate;
            Token = cancellationToken;
            return Task.FromResult(Reports);
        }

        public Task AddAsync(WeeklyTaskReportDto report, CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            Reports.Add(report);
            return Task.CompletedTask;
        }
    }

    private sealed class MovementRepositoryStub : IAccountMovementRepository
    {
        public List<AccountMovementDto> Movements { get; } = [];
        public decimal Balance { get; init; }
        public int TotalCount { get; init; }
        public Exception? Failure { get; init; }
        public AccountMovementDto? Added { get; private set; }
        public CancellationToken Token { get; private set; }
        public Guid UserId { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }

        public Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserId = userId;
            Token = cancellationToken;
            return Task.FromResult(Balance);
        }

        public Task<List<AccountMovementDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserId = userId;
            Token = cancellationToken;
            return Failure != null ? Task.FromException<List<AccountMovementDto>>(Failure) : Task.FromResult(Movements);
        }

        public Task<(List<AccountMovementDto> Movements, int TotalCount)> GetPageAsync(
            Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            UserId = userId;
            Page = page;
            PageSize = pageSize;
            Token = cancellationToken;
            return Failure != null
                ? Task.FromException<(List<AccountMovementDto>, int)>(Failure)
                : Task.FromResult((Movements, TotalCount));
        }

        public Task AddAsync(AccountMovementDto movement, CancellationToken cancellationToken)
        {
            Added = movement;
            Token = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
