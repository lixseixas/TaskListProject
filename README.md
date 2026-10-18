# TaskListProject

A comprehensive task management system built with .NET 10, Angular, and modern architectural patterns including CQRS and DDD.

## Project Overview

TaskListProject is a multi-project solution for managing tasks with reporting capabilities, authentication, and message queue integration. The system demonstrates modern software architecture principles including Clean Architecture, CQRS pattern, and Domain-Driven Design concepts.

## Architecture

The solution follows a layered architecture with clear separation of concerns:

- **Domain Layer**: Core business entities and logic (TaskListProject.Domain)
- **Infrastructure Layer**: Data access and external services (TaskListProject.InfraStructure)
- **Application Layer**: CQRS requests, handlers, validators, results and data-access interfaces (TaskListProject.Application)
- **API Layer**: REST controllers with HTTP request/response mapping (TaskProjectApi)
- **Presentation Layer**: Web UI (TaskProjectWeb) - lightweight MVC application that consumes the API
- **Frontend**: Angular client application (TaskListAngular)

## Technical Concepts

### CQRS (Command Query Responsibility Segregation)

CQRS is a pattern that separates read and write operations into different models. In this project:

- **Commands**: Operations that modify state (CreateTaskCommand, UpdateTaskCommand)
- **Queries**: Operations that read data (GetTasksQuery, GetTaskByIdQuery, GetSummarizedTasksQuery)
- **Handlers**: Process commands and queries using MediatR
- **Benefits**: Improved performance, scalability, and maintainability by optimizing read and write paths independently

All task, movement and weekly-report use cases live in Application, grouped by
feature. `AddApplication()` registers their handlers and existing validators.
Controllers send requests through `ISender` and map Application results to
dedicated HTTP models; neither handlers nor their results depend on API models.
Web keeps its own ViewModels and continues to call the same HTTP routes.

Task persistence uses separate `ITasksQueries` and `ITasksCommands` ports,
implemented by Infrastructure with scoped EF contexts and asynchronous I/O.
Create and update are explicit commands, not a shared method controlled by an
`Inclusion` string. Updates modify the tracked entity and return 404 when missing.
Successful updates still return 204; creation returns the persisted ID in both
the body and Location. DateTime fields and JSON wrappers remain unchanged for
existing Web, React, Angular and consumer compatibility.

Unexpected CQRS failures propagate to the API exception handler and return
ProblemDetails with a trace ID instead of empty successful payloads. Existing
business-rule rejections return 400. This is the minimum error-handling support
for the CQRS migration, not completion of the broader error-handling roadmap.
The existing validators are registered but a validation pipeline is deliberately
deferred to TD-06: its positive-amount rule must first be reconciled with signed
debits. Authentication/authorization changes, scheduling atomicity, pagination
and generating weekly reports from tasks remain separate backlog items.

### DDD (Domain-Driven Design)

Domain-Driven Design principles are applied throughout the project:

- **Domain Entities**: Rich domain models with business logic (TaskDto, UserLoginDto, WeeklyTaskReportDto)
- **Value Objects**: Immutable objects representing domain concepts
- **Aggregates**: Clusters of domain objects treated as a unit
- **Repositories**: Data access abstractions (TasksQueries)
- **Benefits**: Better alignment with business domain, improved code organization, and maintainability

### Other Patterns

- **Dependency Injection**: All services are injected via constructor injection
- **MediatR**: Mediator pattern for in-process messaging
- **FluentValidation**: Declarative validation for commands and queries
- **Mapster**: Object-to-object mapping for DTOs
- **Entity Framework Core**: ORM for database operations

## Projects

### TaskProjectWeb

ASP.NET Core MVC web application (.NET 10) serving as the main task management interface. This is a lightweight presentation layer that consumes the TaskReportApi for all data operations.

**Features:**
- Task CRUD operations with validation
- User authentication via JWT tokens (cookie-based)
- Task scheduling with time validation
- Weekly task reporting
- RabbitMQ integration for async processing
- HTTP client with automatic JWT token injection

**Key Functions:**
- `List()`: Display all tasks with filtering (calls API)
- `Include()`: Create new tasks with validation (calls API)
- `Edit()`: Update existing tasks (calls API)
- `ListHoursPerDay()`: Summarize tasks by date range (calls API)
- `SendWeeklyTaskReport()`: Publish weekly reports to RabbitMQ

**Technology Stack:**
- .NET 10
- ASP.NET Core MVC
- HttpClient with DelegatingHandler for JWT auth
- RabbitMQ.Client 7.2
- log4net 3.3

### TaskReportApi

ASP.NET Core Web API (.NET 10) providing REST endpoints for task management and reporting. Controllers delegate to Application CQRS handlers; Infrastructure implements Application data-access interfaces and owns all database operations.

**Features:**
- Task CRUD endpoints with CQRS pattern
- Task summarization and reporting endpoints
- JWT authentication and authorization
- Swagger/OpenAPI documentation
- CORS support for Angular frontend and TaskProjectWeb
- Infrastructure repositories for data access (no direct EF Core package references)
- MediatR for CQRS command/query handling
- FluentValidation validators in Application (pipeline integration planned in TD-06)
- Mapster for object mapping

**Endpoints:**
- `GET /api/tasks`: Get all tasks
- `GET /api/tasks/{id}`: Get task by ID
- `POST /api/tasks/include`: Create new task
- `PUT /api/tasks/{id}`: Update existing task
- `GET /api/tasks/summarized`: Get summarized tasks by date range
- `GET /api/tasks/validate-superposition`: Validate task time superposition
- `POST /api/login`: Authenticate and get JWT token
- `GET /api/TaskReport/weekly`: Read persisted weekly report snapshots
- `POST /api/TaskReport/weekly`: Insert a weekly report snapshot
- `GET /api/AccountMovements/by-user/{userId}`: Get a movement page
- `POST /api/AccountMovements`: Create a movement
- `GET /api/AccountMovements/balance/{userId}`: Calculate balance

**Technology Stack:**
- .NET 10
- ASP.NET Core Web API
- MediatR 14.2
- FluentValidation 11.11
- Mapster 10.0
- JWT Authentication

### TaskListAngular

Angular frontend application for task management and reporting.

**Features:**
- Task list view with grouping
- Login/authentication with JWT
- Task report visualization
- HTTP interceptors for token injection
- Route guards for protected routes

**Technology Stack:**
- Angular 18+
- TypeScript
- RxJS for reactive programming
- TailwindCSS for styling

### RabbitMqConsumerReceive

Console application for consuming messages from RabbitMQ.

**Features:**
- Consumes weekly task report messages
- Processes reports asynchronously
- Integration with task reporting system

### Shared Projects

**TaskListProject.Domain**
- Domain entities (TaskDto, UserLoginDto, WeeklyTaskReportDto)
- Business logic and validation rules
- Domain-specific types and interfaces

**TaskListProject.InfraStructure**
- Entity Framework DbContext
- Database migrations
- Data access queries (TasksQueries, UserQueries)
- External service integrations
- Implements Application data-access interfaces and registers SQL Server and repositories through `AddInfrastructure(configuration)`
- Owns production EF Core packages; references Application and Domain (tests also use EF Core SQLite)

**TaskListProject.Application**
- CQRS command and query handlers
- Application services
- Business logic orchestration
- Mapping configurations
- Data-access interfaces; references Domain, not Infrastructure or EF Core

EF Core remains a transitive runtime dependency of the API through Infrastructure.
Database migrations remain in Infrastructure; this refactor does not change the schema.
Run EF tooling against `TaskListProject.InfraStructure` using its existing design-time
context factory, rather than the API as an EF design-time startup project.
Account balance and movement-list queries now log and propagate database failures
instead of returning a misleading zero balance or empty successful response.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 18+ (for Angular)
- SQL Server (or compatible database)
- RabbitMQ server (optional, for messaging features)

### Database Setup

1. Update connection string in `appsettings.json` files
2. Run migrations:
   ```bash
   cd TaskListProject.InfraStructure
   dotnet ef database update
   ```

### Running the Applications

**TaskProjectWeb:**
```bash
cd TaskProjectWeb
dotnet run
```

**TaskProjectApi:**
```bash
cd TaskProjectApi
dotnet run
```

**TaskListAngular:**
```bash
cd TaskListAngular
npm install
ng serve
```

**RabbitMqConsumerReceive:**
```bash
cd RabbitMqConsumerReceive
dotnet run
```

## Screenshots

### Home
![Home](https://github.com/lixseixas/Agenda02/blob/main/TaskProject/ImgPreview/sis_home.png)

### Task List
![List](https://github.com/lixseixas/Agenda02/blob/main/TaskProject/ImgPreview/sis_list.png)

### Hours Summary
![Hours](https://github.com/lixseixas/Agenda02/blob/main/TaskProject/ImgPreview/sis_hours.png)

### Add/Edit Task
![Add/Edit](https://github.com/lixseixas/Agenda02/blob/main/TaskProject/ImgPreview/sis_include.png)

## Development Roadmap

See [technicalDebits.md](.Tasks/technicalDebits.md) for technical debts and the implementation roadmap.

## CQRS verification

Run the focused boundary, persistence and HTTP contract tests:

```powershell
dotnet test TestProject\TestProject.csproj --filter "FullyQualifiedName~DataAccessBoundaryTests|FullyQualifiedName~DbTests|FullyQualifiedName~CqrsApiTests"
dotnet build TaskProjectWeb\TaskProjectWeb.csproj --no-restore
```

The HTTP tests host the real API, mediator, JWT validation and task/report
persistence using an isolated SQLite database; movement persistence is stubbed.
They cover routes, JSON fields, OpenAPI, create/read/update, missing resources,
rule rejection and database failures. SQLite does not replace SQL Server
concurrency/migration tests (TD-07/TD-11). Legacy `AccountMovementsTests` still
creates a context without a provider and is not included in this focused suite.
Existing dependency-audit and nullable warnings are not resolved by TD-05.

For manual checks, use [TaskReportApi.http](TaskProjectApi/TaskReportApi.http).
Replace the login credentials, token and IDs with local values. No database
migration or new runtime configuration is required by this CQRS refactor.

## License

This project is developed as a demonstration of modern .NET development practices and architectural patterns.
