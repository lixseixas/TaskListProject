using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TaskListProject.Application;
using TaskListProject.Application.Data;
using TaskListProject.Infrastructure;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskListProject.Application.AccountMovements.Commands;
using TaskListProject.Application.AccountMovements.Queries;
using TaskListProject.Application.Common;
using TaskListProject.Application.Tasks.Commands;
using TaskListProject.Application.Tasks.Queries;
using TaskListProject.Application.WeeklyTaskReports.Commands;
using TaskListProject.Application.WeeklyTaskReports.Queries;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskReportApi.Controllers;

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
        services.AddApplication();
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
        Assert.That(scope.ServiceProvider.GetRequiredService<ITasksCommands>(), Is.TypeOf<TasksCommands>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IUserQueries>(), Is.TypeOf<UserQueries>());
        Assert.That(scope.ServiceProvider.GetRequiredService<LoginHandler>(), Is.Not.Null);
        Assert.That(scope.ServiceProvider.GetRequiredService<IRequestHandler<CreateTaskCommand, TaskListProject.Application.Tasks.TaskResult>>(),
            Is.TypeOf<CreateTaskCommandHandler>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IValidator<GetSummarizedTasksQuery>>(),
            Is.TypeOf<GetSummarizedTasksQueryValidator>());
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
    public void SqlServerModelStillMatchesMigrationsSnapshot()
    {
        var options = new DbContextOptionsBuilder<TaskContext>()
            .UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True").Options;
        using var context = new TaskContext(options);
        Assert.That(context.Database.HasPendingModelChanges(), Is.False);
    }

    [Test]
    public void ApplicationHasNoInfrastructureOrEfAssemblyReferences()
    {
        var references = typeof(CreateTaskCommand).Assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();
        Assert.That(references, Has.None.EqualTo("TaskListProject.InfraStructure"));
        Assert.That(references, Has.None.EqualTo("TaskProjectApi"));
        Assert.That(references, Has.None.EqualTo("TaskProjectWeb"));
        Assert.That(references.Any(n => n != null && n.StartsWith("Microsoft.EntityFrameworkCore")), Is.False);
    }

    [Test]
    public void CqrsHandlersBelongToApplicationAndControllersOnlyDependOnSender()
    {
        var application = typeof(CreateTaskCommand).Assembly;
        var api = typeof(TasksController).Assembly;
        var handlers = application.GetTypes().Where(type => type.GetInterfaces().Any(contract =>
            contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))).ToList();
        Assert.That(handlers.Count, Is.GreaterThanOrEqualTo(11));
        foreach (var handler in handlers)
        {
            var contract = handler.GetInterfaces().Single(type =>
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            var result = contract.GetGenericArguments()[1];
            var resultTypes = result.IsGenericType ? result.GetGenericArguments() : new[] { result };
            Assert.That(resultTypes.All(type => type.Assembly == application || type.IsPrimitive), Is.True,
                $"{handler.Name} must return an Application result or a primitive, not an API/domain entity");
        }
        Assert.That(api.GetTypes().Any(type => type.GetInterfaces().Any(contract =>
            contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))), Is.False);
        foreach (var controller in new[] { typeof(TasksController), typeof(TaskReportController), typeof(AccountMovementsController) })
        {
            Assert.That(controller.GetConstructors().Single().GetParameters().Select(parameter => parameter.ParameterType),
                Is.EqualTo(new[] { typeof(ISender) }));
        }

        Assert.That(typeof(ITasksQueries).GetMethods().Any(method =>
            method.GetParameters().Any(parameter => parameter.ParameterType.IsByRef)), Is.False);
        Assert.That(typeof(ITasksQueries).GetMethods().Select(method => method.Name),
            Has.None.EqualTo(nameof(ITasksCommands.AddAsync)));
        Assert.That(typeof(TaskDto).GetProperty("Inclusion"), Is.Null);
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
        var handler = new GetWeeklyTaskReportsQueryHandler(repository);
        var results = await handler.Handle(new GetWeeklyTaskReportsQuery(
            report.WeekStartDate.AddHours(12), report.WeekEndDate.AddHours(12)), cancellation.Token);
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
        var handler = new InsertWeeklyTaskReportCommandHandler(repository);
        var result = await handler.Handle(new InsertWeeklyTaskReportCommand
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
        var handler = new InsertWeeklyTaskReportCommandHandler(repository);
        Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(new InsertWeeklyTaskReportCommand
        {
            TotalTasks = 1, CompletedTasks = 2
        }, CancellationToken.None));
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
        Assert.ThrowsAsync<BusinessRuleException>(() => handler.Handle(
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
