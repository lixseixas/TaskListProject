Project TaskProject

Create table with AccountMovements with the values
Id, UserId (userid from UserLogin), Amount, Type, Date, Description(nullables)

Create AccountMovement Controller with the methods:
- GetMovementsByUserId
- CreateMovement
- Create a method to calculate the balance
    - Limit the account balance to 0
- Limit the account movements to 10 per page

 
Implementation Summary
✅ CQRS Pattern with MediatR
Query: GetMovementsByUserIdQuery - Retrieves movements with pagination (10 per page)
Command: CreateMovementCommand - Creates new account movements
Query: CalculateBalanceQuery - Calculates account balance with limit to 0
✅ FluentValidation
GetMovementsByUserIdQueryValidator - Validates pagination parameters
CreateMovementCommandValidator - Validates movement data (amount, type validation)
CalculateBalanceQueryValidator - Validates user ID
✅ Mapster Configuration
Added mappings between AccountMovementDto and AccountMovementModel
Integrated with existing Mapster configuration
✅ Dependency Injection
All handlers use constructor injection for dependencies
No new operators for service instantiation
Uses DI for TaskContext, ILogger, and IMediator
✅ AccountMovementsController
GET /api/accountmovements/by-user/{userId} - Get movements with pagination
POST /api/accountmovements - Create new movement
GET /api/accountmovements/balance/{userId} - Calculate balance
✅ Database Schema
Created AccountMovements table with fields: Id, UserId, Amount, Type, Date, Description
Applied migration successfully
✅ Balance Calculation Logic
Calculates credits and debits
Limits balance to minimum of 0 as required
The implementation follows clean architecture principles with proper separation of concerns between Domain, Infrastructure, and API layers, using the CQRS pattern to separate read and write operations.