# TaskListProject (adapted)

This repository is derived from an earlier TaskList application that managed and persisted task lists.

TaskProjectApi (Backend API)
This section covers the backend API, which handles task management, account movements, reporting, and related data operations. The API is built using ASP.NET Core and follows a CQRS pattern with EF Core for data persistence.
Functionality includes:
- Task management (create, update, delete tasks)
- Account movements (track and manage financial transactions)
- Reporting (generate summaries and reports based on tasks and account movements)

TaskProjectReact (Frontend)
This section covers the frontend React application, which provides the user interface for managing tasks and viewing account movements and reports. The frontend communicates with the backend API to perform these operations.  
Functionality includes:
- Mocked login page
- Task management (create, update, delete tasks)
- Account movements (view and manage financial transactions)
- Reporting (view summaries and reports based on tasks and account movements)

Exemplo inicial:
![Print tela exemplo](Print%20tela%20exemplo.png)

AI Use:
.devin folder contains:
- Workflows that read .tasks folder and calls the cshar-developer agent
.tasks folder contains:
- Task files that describe the tasks to be done, like a primitive spec driven development approach
- I commited this A.I files to show how we can use AI to develop this project.


Instructions
Frontend (React)
- A React frontend is included at TaskListProjectReact. 
- Quick start (React):
  1. cd TaskListProjectReact
  2. npm install
  3. npm start

Backend (API)
1. Update connection strings in appsettings.json for the API or infrastructure projects.
2. Run migrations from the infrastructure project if needed:
   dotnet ef database update --project TaskListProject.InfraStructure
3. Run the API project(s):
   dotnet run --project TaskProjectApi


License
This repository is provided as-is for development and learning purposes.
