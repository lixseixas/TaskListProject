using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskListProject.Application.AccountMovements.Commands;
using TaskListProject.Application.AccountMovements.Queries;
using TaskReportApi.Models;

namespace TaskReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountMovementsController(ISender sender) : ControllerBase
{
    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(AccountMovementsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountMovementsResponseModel>> GetMovementsByUserId(
        Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetMovementsByUserIdQuery { UserId = userId, Page = page, PageSize = pageSize }, cancellationToken);
        return Ok(result.Adapt<AccountMovementsResponseModel>());
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountMovementModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountMovementModel>> CreateMovement(
        [FromBody] CreateMovementRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(request.Adapt<CreateMovementCommand>(), cancellationToken);
        return CreatedAtAction(nameof(CalculateBalance), new { userId = result.UserId },
            result.Adapt<AccountMovementModel>());
    }

    [HttpGet("balance/{userId:guid}")]
    [ProducesResponseType(typeof(AccountBalanceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountBalanceModel>> CalculateBalance(Guid userId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CalculateBalanceQuery { UserId = userId }, cancellationToken);
        return Ok(result.Adapt<AccountBalanceModel>());
    }
}
