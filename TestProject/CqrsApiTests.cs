using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using TaskListProject.Application.Data;
using TaskListProject.Infrastructure.Data;
using TaskProject.Domain.Entities;
using TaskReportApi.Models;

namespace TestProject;

public class CqrsApiTests
{
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new ApiFactory();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TaskContext>().Database.EnsureCreatedAsync();
        var token = new JwtSecurityToken(
            issuer: "cqrs-tests", audience: "cqrs-tests",
            claims: [new Claim(ClaimTypes.Name, "test-user")],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.SigningKey)),
                SecurityAlgorithms.HmacSha256));
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

    [TearDown]
    public async Task Cleanup()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task TaskRoutesPreservePayloadsAndPersistEdits()
    {
        var model = NewTask();
        using var created = await _client.PostAsJsonAsync("/api/tasks/include", model);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var saved = await created.Content.ReadFromJsonAsync<TaskModel>();
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(saved, Is.EqualTo(model with { Id = saved.Id }));
        Assert.That(created.Headers.Location!.ToString(), Does.EndWith($"/api/Tasks/{saved.Id}").IgnoreCase);
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Assert.That(body.RootElement.EnumerateObject().Select(property => property.Name), Is.EquivalentTo(
            new[] { "id", "title", "description", "date", "initialHour", "finalHour", "priority", "ended" }));
        Assert.That(await _client.GetFromJsonAsync<TaskModel>(created.Headers.Location), Is.EqualTo(saved));
        var list = await _client.GetFromJsonAsync<TaskListModel>("/api/tasks");
        Assert.That(list!.TaskList.Single(), Is.EqualTo(saved));

        var updated = saved with
        {
            Id = Guid.NewGuid(), Title = "Edited", Description = "Edited description",
            Date = saved.Date.AddDays(1), InitialHour = saved.InitialHour.AddHours(1),
            FinalHour = saved.FinalHour.AddHours(2), Priority = 3, Ended = true
        };
        using var edited = await _client.PutAsJsonAsync($"/api/tasks/{saved.Id}", updated);
        Assert.That(edited.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(await _client.GetFromJsonAsync<TaskModel>(created.Headers.Location),
            Is.EqualTo(updated with { Id = saved.Id }));
        using var missing = await _client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
        Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        using var missingUpdate = await _client.PutAsJsonAsync($"/api/tasks/{Guid.NewGuid()}", model);
        Assert.That(missingUpdate.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That((await _client.GetFromJsonAsync<TaskListModel>("/api/tasks"))!.TaskList.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task CreationPreservesClientIdAndMvcValidation()
    {
        var model = NewTask() with { Id = Guid.NewGuid() };
        using var created = await _client.PostAsJsonAsync("/api/tasks/include", model);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That((await created.Content.ReadFromJsonAsync<TaskModel>())!.Id, Is.EqualTo(model.Id));
        using var invalid = await _client.PostAsJsonAsync("/api/tasks/include", model with { Title = "" });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That((await _client.GetFromJsonAsync<TaskListModel>("/api/tasks"))!.TaskList.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task SummaryAndSuperpositionKeepExistingShapes()
    {
        using var created = await _client.PostAsJsonAsync("/api/tasks/include", NewTask() with { Ended = true });
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var summary = await _client.GetFromJsonAsync<SearchTaskModel>(
            "/api/tasks/summarized?initialDate=2026-10-17&finalDate=2026-10-17");
        Assert.That(summary!.InitialDate, Is.EqualTo(new DateTime(2026, 10, 17)));
        Assert.That(summary.FinalDate, Is.EqualTo(summary.InitialDate));
        var day = summary.ListTasksSummarized.Single();
        Assert.Multiple(() =>
        {
            Assert.That(day.Hours, Is.EqualTo("01:00:00"));
            Assert.That(day.AverageHours, Is.EqualTo("01:00:00"));
            Assert.That(day.TotalTasks, Is.EqualTo(1));
            Assert.That(day.PercentualConcludedTasks, Is.EqualTo(100));
        });
        var available = await _client.GetFromJsonAsync<bool>(
            "/api/tasks/validate-superposition?id=00000000-0000-0000-0000-000000000000" +
            "&date=2026-10-17&initial=2026-10-17T09:00:00&final=2026-10-17T10:00:00");
        Assert.That(available, Is.False);
    }

    [Test]
    public async Task WeeklyReportRoutesPreserveSnapshotsAndRejectInvalidCounts()
    {
        var report = new WeeklyTaskReportModel
        {
            WeekStartDate = new DateTime(2026, 10, 12), WeekEndDate = new DateTime(2026, 10, 18),
            WeekNumber = 42, Year = 2026, TotalTasks = 10, CompletedTasks = 8,
            PendingTasks = 2, CompletionPercentage = 80
        };
        using var created = await _client.PostAsJsonAsync("/api/TaskReport/weekly", report);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var saved = await created.Content.ReadFromJsonAsync<WeeklyTaskReportModel>();
        Assert.That(saved!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(saved, Is.EqualTo(report with { Id = saved.Id }));
        var list = await _client.GetFromJsonAsync<List<WeeklyTaskReportModel>>(created.Headers.Location);
        Assert.That(list!.Single(), Is.EqualTo(saved));
        using var invalid = await _client.PostAsJsonAsync("/api/TaskReport/weekly",
            report with { CompletedTasks = 11 });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        using var inverted = await _client.GetAsync("/api/TaskReport/weekly?startDate=2026-10-18&endDate=2026-10-12");
        Assert.That(inverted.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task MovementRoutesKeepRequestAndResponseContractsIncludingDebits()
    {
        var userId = Guid.NewGuid();
        var request = new CreateMovementRequest
        {
            UserId = userId, Amount = -5, Type = "Debit", Date = new DateTime(2026, 10, 17),
            Description = "Test debit"
        };
        using var created = await _client.PostAsJsonAsync("/api/AccountMovements", request);
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var movement = await created.Content.ReadFromJsonAsync<AccountMovementModel>();
        Assert.That(movement!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(movement.UserId, Is.EqualTo(userId));
        Assert.That(movement.Amount, Is.EqualTo(-5));
        Assert.That(movement.Type, Is.EqualTo("Debit"));
        Assert.That(movement.Description, Is.EqualTo(request.Description));
        Assert.That(movement.Date, Is.EqualTo(request.Date));
        var page = await _client.GetFromJsonAsync<AccountMovementsResponseModel>(
            $"/api/AccountMovements/by-user/{userId}?page=1&pageSize=10");
        Assert.Multiple(() =>
        {
            Assert.That(page!.Movements.Single().Id, Is.EqualTo(movement.Id));
            Assert.That(page.TotalCount, Is.EqualTo(1));
            Assert.That(page.TotalPages, Is.EqualTo(1));
            Assert.That(page.Page, Is.EqualTo(1));
            Assert.That(page.PageSize, Is.EqualTo(10));
        });
        var balance = await _client.GetFromJsonAsync<AccountBalanceModel>(created.Headers.Location);
        Assert.That(balance!.UserId, Is.EqualTo(userId));
        Assert.That(balance.Balance, Is.Zero);
        using var invalid = await _client.PostAsJsonAsync("/api/AccountMovements", request with { Amount = -200 });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("/api/tasks")]
    [TestCase("/api/TaskReport/weekly?startDate=2026-10-12&endDate=2026-10-18")]
    public async Task ProtectedRoutesStillRequireAuthentication(string route)
    {
        _client.DefaultRequestHeaders.Authorization = null;
        using var response = await _client.GetAsync(route);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [TestCase("/api/tasks")]
    [TestCase("/api/tasks/summarized?initialDate=2026-10-17&finalDate=2026-10-17")]
    public async Task DatabaseFailuresAreNotSuccessfulEmptyPayloads(string route)
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TaskContext>().Database.ExecuteSqlRawAsync("DROP TABLE Tasks");
        using var response = await _client.GetAsync(route);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/problem+json"));
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(problem.RootElement.GetProperty("status").GetInt32(), Is.EqualTo(500));
        Assert.That(problem.RootElement.GetProperty("traceId").GetString(), Is.Not.Empty);
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("SQLite"));
    }

    [Test]
    public async Task SwaggerStillDescribesAllExistingRoutesAndBearerSecurity()
    {
        using var response = await _client.GetAsync("/swagger/v1/swagger.json");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        var paths = root.GetProperty("paths");
        foreach (var (route, method) in new[]
        {
            ("/api/Tasks", "get"), ("/api/Tasks/{id}", "get"), ("/api/Tasks/{id}", "put"),
            ("/api/Tasks/include", "post"), ("/api/Tasks/summarized", "get"),
            ("/api/Tasks/validate-superposition", "get"), ("/api/TaskReport/weekly", "get"),
            ("/api/TaskReport/weekly", "post"), ("/api/AccountMovements", "post"),
            ("/api/AccountMovements/by-user/{userId}", "get"), ("/api/AccountMovements/balance/{userId}", "get")
        })
        {
            Assert.That(paths.GetProperty(route).TryGetProperty(method, out _), Is.True, $"{method} {route}");
        }

        Assert.That(paths.GetProperty("/api/Tasks").GetProperty("get").GetProperty("security").GetArrayLength(),
            Is.EqualTo(1));
        var bearer = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.That(bearer.GetProperty("type").GetString(), Is.EqualTo("http"));
        Assert.That(bearer.GetProperty("scheme").GetString(), Is.EqualTo("bearer"));
        var requirement = paths.GetProperty("/api/Tasks").GetProperty("get").GetProperty("security")[0];
        Assert.That(requirement.TryGetProperty("Bearer", out var scopes), Is.True);
        Assert.That(scopes.GetArrayLength(), Is.Zero);
        var login = paths.GetProperty("/api/Login").GetProperty("post");
        Assert.That(!login.TryGetProperty("security", out var loginSecurity) ||
            loginSecurity.GetArrayLength() == 0, Is.True);
        var schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.That(schemas.GetProperty("CreateMovementRequest").GetProperty("properties")
            .EnumerateObject().Select(property => property.Name),
            Is.EquivalentTo(new[] { "userId", "amount", "type", "date", "description" }));
        Assert.That(schemas.TryGetProperty("TaskDto", out _), Is.False);
    }

    private static TaskModel NewTask() => new()
    {
        Title = "Task", Description = "Description", Date = new DateTime(2026, 10, 17),
        InitialHour = new DateTime(2026, 10, 17, 9, 0, 0),
        FinalHour = new DateTime(2026, 10, 17, 10, 0, 0), Priority = 1
    };

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        internal const string SigningKey = "CQRS-tests-only-signing-key-not-for-production-12345";
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();
            builder.UseEnvironment("Development");
            builder.UseSetting("Jwt:Key", SigningKey);
            builder.UseSetting("Jwt:Issuer", "cqrs-tests");
            builder.UseSetting("Jwt:Audience", "cqrs-tests");
            builder.UseSetting("ConnectionStrings:LocalDbConnection", "Server=unused;Database=unused");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TaskContext>();
                services.RemoveAll<DbContextOptions<TaskContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<TaskContext>>();
                services.AddDbContext<TaskContext>(options => options.UseSqlite(_connection));
                services.RemoveAll<IAccountMovementRepository>();
                services.AddSingleton<IAccountMovementRepository, MovementRepository>();
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class MovementRepository : IAccountMovementRepository
    {
        private readonly List<AccountMovementDto> _movements = [];

        public Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult(100m);

        public Task<List<AccountMovementDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(_movements.Where(movement => movement.UserId == userId).ToList());

        public async Task<(List<AccountMovementDto> Movements, int TotalCount)> GetPageAsync(
            Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var movements = await GetByUserIdAsync(userId, cancellationToken);
            return (movements.Skip((page - 1) * pageSize).Take(pageSize).ToList(), movements.Count);
        }

        public Task AddAsync(AccountMovementDto movement, CancellationToken cancellationToken)
        {
            _movements.Add(movement);
            return Task.CompletedTask;
        }
    }
}
