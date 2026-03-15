using Application.BacktestIntegrity.Commands.CreateBacktestSession;
using Application.BacktestIntegrity.Commands.SubmitBacktestMetrics;
using Application.BacktestIntegrity.Commands.SubmitIntegrityAnswers;
using Application.BacktestIntegrity.Queries.GetBacktestStatus;
using Application.Dto;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TradingJournalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BacktestController : ControllerBase
{
    private readonly IMediator _mediator;

    public BacktestController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId
    {
        get
        {
            return Guid.Parse("bb048163-62ee-4c37-a6ba-6b1b3ba74c1b");
            var raw = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (raw is null || !Guid.TryParse(raw, out var guid))
                throw new UnauthorizedAccessException("User ID claim not found.");
        }
    }

    [HttpGet("debug-claims")]
    [AllowAnonymous]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated, Claims = claims });
    }

    [HttpPost("session")]
    public async Task<ServiceResponse<BacktestSessionDto>> CreateSession()
        => await _mediator.Send(new CreateBacktestSessionCommand(CurrentUserId));

    [HttpGet("status")]
    public async Task<ServiceResponse<BacktestStatusDto>> GetStatus()
        => await _mediator.Send(new GetBacktestStatusQuery(CurrentUserId));

    [HttpPost("metrics")]
    public async Task<ServiceResponse<BacktestStatusDto>> SubmitMetrics(
        [FromBody] SubmitBacktestMetricsRequest request)
        => await _mediator.Send(new SubmitBacktestMetricsCommand(
            CurrentUserId, request.TotalTrades, request.WinRate, request.AvgRR));

    [HttpPost("answers")]
    public async Task<ServiceResponse<BacktestStatusDto>> SubmitAnswers(
        [FromBody] SubmitIntegrityAnswersRequest request)
        => await _mediator.Send(new SubmitIntegrityAnswersCommand(
            CurrentUserId,
            request.Answers.Select(a => new IntegrityAnswerInput(a.QuestionIndex, a.Answer, a.Reason)).ToList()));
}

public record SubmitBacktestMetricsRequest(int TotalTrades, decimal WinRate, decimal AvgRR);
public record SubmitIntegrityAnswersRequest(List<AnswerInput> Answers);
public record AnswerInput(int QuestionIndex, bool Answer, string? Reason);
