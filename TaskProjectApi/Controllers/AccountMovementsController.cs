using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Extensions.Logging;
using TaskReportApi.Models;
using MediatR;
using TaskReportApi.CQRS.AccountMovements.Queries;
using TaskReportApi.CQRS.AccountMovements.Commands;

namespace TaskReportApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class AccountMovementsController : ControllerBase
{
    private readonly ILogger<AccountMovementsController> _logger;
    private readonly IMediator _mediator;

    public AccountMovementsController(
        ILogger<AccountMovementsController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("by-user/{userId:guid}")]
    [ProducesResponseType(typeof(AccountMovementsResponseModel), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AccountMovementsResponseModel>> GetMovementsByUserId(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetMovementsByUserIdQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movements for UserId {UserId}", userId);
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountMovementModel), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AccountMovementModel>> CreateMovement([FromBody] CreateMovementCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(CalculateBalance), new { userId = result.UserId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movement for UserId {UserId}", command.UserId);
            if (ex is InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpGet("balance/{userId:guid}")]
    [ProducesResponseType(typeof(AccountBalanceModel), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AccountBalanceModel>> CalculateBalance(Guid userId)
    {
        try
        {
            var query = new CalculateBalanceQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating balance for UserId {UserId}", userId);
            return StatusCode(500, "An error occurred");
        }
    }
}
