# TaskListProject

A comprehensive task management system built with .NET 10, Angular, and modern architectural patterns including CQRS and DDD.

## Project Overview

TaskListProject is a multi-project solution for managing tasks with reporting capabilities, authentication, and message queue integration. The system demonstrates modern software architecture principles including Clean Architecture, CQRS pattern, and Domain-Driven Design concepts.

## Architecture

The solution follows a layered architecture with clear separation of concerns:

- **Domain Layer**: Core business entities and logic (TaskListProject.Domain)
- **Infrastructure Layer**: Data access and external services (TaskListProject.InfraStructure)
- **Application Layer**: Application services and CQRS handlers (TaskListProject.Application)
- **API Layer**: REST API with CQRS, data access, and business logic (TaskReportApi)
- **Presentation Layer**: Web UI (TaskProjectWeb) - lightweight MVC application that consumes the API
- **Frontend**: Angular client application (TaskListAngular)

## Technical Concepts

### CQRS (Command Query Responsibility Segregation)

CQRS is a pattern that separates read and write operations into different models. In this project:

- **Commands**: Operations that modify state (CreateTaskCommand, UpdateTaskCommand)
- **Queries**: Operations that read data (GetTasksQuery, GetTaskByIdQuery, GetSummarizedTasksQuery)
- **Handlers**: Process commands and queries using MediatR
- **Benefits**: Improved performance, scalability, and maintainability by optimizing read and write paths independently

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

ASP.NET Core Web API (.NET 10) providing REST endpoints for task management and reporting. This is the central API layer that handles all data access, CQRS operations, and business logic.

**Features:**
- Task CRUD endpoints with CQRS pattern
- Task summarization and reporting endpoints
- JWT authentication and authorization
- Swagger/OpenAPI documentation
- CORS support for Angular frontend and TaskProjectWeb
- Entity Framework Core for data access
- MediatR for CQRS command/query handling
- FluentValidation for request validation
- Mapster for object mapping

**Endpoints:**
- `GET /api/tasks`: Get all tasks
- `GET /api/tasks/{id}`: Get task by ID
- `POST /api/tasks`: Create new task
- `PUT /api/tasks/{id}`: Update existing task
- `GET /api/tasks/summarized`: Get summarized tasks by date range
- `GET /api/tasks/validate-superposition`: Validate task time superposition
- `POST /api/login`: Authenticate and get JWT token

**Technology Stack:**
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core 10.0
- MediatR 13.0
- FluentValidation 11.3
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

**TaskListProject.Application**
- CQRS command and query handlers
- Application services
- Business logic orchestration
- Mapping configurations

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

**TaskReportApi:**
```bash
cd TaskReportApi
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

See [technical-debts.md](Docs/technical-debts.md) for detailed technical debts and improvement roadmap.

## License

This project is developed as a demonstration of modern .NET development practices and architectural patterns.
