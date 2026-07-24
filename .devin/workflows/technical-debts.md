---
description: TaskListProject Technical Debts and Roadmap
---

# TaskListProject Technical Debts and Roadmap

## Current Issues and Technical Debt

### 1. **Entity Framework Version Inconsistency** ⚠️
- TaskProject uses EF Core 5.0.11
- TaskReportApi uses EF Core 8.0.0
- **Impact**: Potential compatibility issues, missing performance improvements and security patches
- **Recommendation**: Upgrade TaskProject to EF Core 8.0.0

### 2. **Duplicate Model Definitions** ⚠️ PARTIALLY RESOLVED
- ✅ Models moved to `TaskListProject.Domain.Entities` (TaskDto, UserLoginDto, WeeklyTaskReportDto, etc.)
- ⚠️ TaskReportApi still has its own models in `TaskReportApi.Models` namespace
- **Impact**: Code duplication, maintenance burden, potential inconsistencies
- **Recommendation**: TaskReportApi should reference and use TaskListProject.Domain.Entities

### 3. **Data Access Pattern Inconsistency** ✅ RESOLVED
- ✅ TaskContext and TasksDal moved to `TaskListProject.Infrastructure.Data`
- ✅ Infrastructure project configured with EF Design tools
- ✅ Migrations moved to Infrastructure project
- **Status**: Data access layer now properly separated in Infrastructure project

### 4. **TaskReportApi Service Implementation Incomplete** ⚠️
- `GetWeeklyTaskReportAsync` only queries existing `WeeklyTasks` table
- Does not aggregate data from `Tasks` table to generate reports
- **Impact**: API doesn't actually generate reports, just retrieves pre-calculated data
- **Recommendation**: Implement proper aggregation logic from Tasks table

### 5. **Missing Database Migrations** ✅ RESOLVED
- ✅ Migrations moved to `TaskListProject.Infrastructure.Migrations`
- ✅ Namespace updated to `TaskListProject.Infrastructure.Migrations`
- ✅ EF Design tools added to Infrastructure project
- **Status**: Migrations now properly located in Infrastructure project

### 6. **No Input Validation in API** ⚠️
- TaskReportApi has empty Validators folder
- **Impact**: Invalid data can be processed
- **Recommendation**: Add validation layer using FluentValidation or DataAnnotations

### 7. **Outdated Startup Pattern** ⚠️
- TaskProject uses `Startup.cs` (older pattern)
- TaskReportApi uses modern top-level statements in `Program.cs`
- **Impact**: Inconsistent project structure
- **Recommendation**: Migrate TaskProject to Program.cs pattern

### 8. **Missing Unit Tests** ⚠️ PARTIALLY RESOLVED
- ✅ TestProject has DbTests.cs with basic tests
- ⚠️ Limited test coverage, needs expansion
- **Impact**: Minimal test coverage, regression risk
- **Recommendation**: Add comprehensive unit tests for business logic

### 9. **Unclear RabbitMQ Integration** ⚠️
- RabbitMQ projects exist with unclear integration
- **Impact**: Unused code, potential confusion
- **Recommendation**: Document purpose or remove if unused

### 10. **Gitignore Overly Broad** ✅ RESOLVED
- ✅ `.gitignore` no longer blocks `.md` files
- **Status**: Documentation can now be committed

## Proposed Modernization Plan

### Phase 1: Foundation (High Priority)
1. ✅ Create shared class library for common models and interfaces (TaskListProject.Domain created)
2. ⚠️ Upgrade TaskProject to EF Core 8.0.0 (still on 5.0.11)
3. ✅ Standardize data access pattern across projects (Infrastructure project created)
4. ✅ Fix .gitignore to allow documentation

### Phase 2: API Enhancement (High Priority)
1. Implement proper weekly report aggregation logic in TaskReportService
2. Add input validation using FluentValidation
3. Make TaskReportApi reference TaskListProject.Domain.Entities instead of local models
4. Add comprehensive error handling and logging

### Phase 3: Web App Modernization (Medium Priority)
1. Migrate TaskProject from Startup.cs to Program.cs pattern
2. Expand unit tests for business logic beyond basic DbTests
3. Improve validation logic in TaskController
4. Consider upgrading EF Core to 8.0.0 for consistency

### Phase 4: Integration & Documentation (Low Priority)
1. Document RabbitMQ integration purpose or remove
2. Add API documentation
3. Create developer guide
4. Set up CI/CD pipeline

## Implementation Approach

To implement these improvements, use the **cssharp** skill which provides expert C# development guidance for modern .NET applications.

The cssharp skill will help with:
- Modern .NET 8.0 best practices
- Entity Framework Core 8.0 migration
- Repository pattern implementation
- API design with proper validation
- Unit testing setup with NUnit
- Object mapping with Mapster
- Clean architecture principles
