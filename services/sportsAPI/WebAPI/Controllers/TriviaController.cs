using Application.Trivia.Commands.AddTriviaQuestion;
using Application.Trivia.Commands.ArchiveTriviaQuestion;
using Application.Trivia.Commands.CreateTriviaSeries;
using Application.Trivia.Commands.PublishTriviaQuestion;
using Application.Trivia.Commands.SubmitTriviaAnswer;
using Application.Trivia.Queries.GetTriviaSeries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace sportsAPI.Controllers;

[Route("api/trivia")]
[ApiController]
public class TriviaController : ControllerBase
{
    private readonly IMediator _mediator;

    public TriviaController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// GET /api/trivia/{organizationId} — List all trivia series with questions and participation counts.
    /// </summary>
    [HttpGet("{organizationId:guid}")]
    public async Task<ServiceResponse<List<TriviaSeriesDto>>> GetSeries(
        Guid organizationId, CancellationToken ct)
        => await _mediator.Send(new GetTriviaSeriesQuery(organizationId), ct);

    /// <summary>
    /// POST /api/trivia/series — Create a new trivia series.
    /// </summary>
    [HttpPost("series")]
    public async Task<ServiceResponse<CreateTriviaSeriesResult>> CreateSeries(
        [FromBody] CreateTriviaSeriesCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);

    /// <summary>
    /// POST /api/trivia/questions — Add a question to an existing series.
    /// </summary>
    [HttpPost("questions")]
    public async Task<ServiceResponse<AddTriviaQuestionResult>> AddQuestion(
        [FromBody] AddTriviaQuestionCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);

    /// <summary>
    /// PUT /api/trivia/questions/{questionId}/publish — Publish a Pending question (Pending → Active).
    /// </summary>
    [HttpPut("questions/{questionId:guid}/publish")]
    public async Task<ServiceResponse<bool>> PublishQuestion(
        Guid questionId, CancellationToken ct)
        => await _mediator.Send(new PublishTriviaQuestionCommand(questionId), ct);

    /// <summary>
    /// PUT /api/trivia/questions/{questionId}/archive — Archive a question (any → Archived).
    /// </summary>
    [HttpPut("questions/{questionId:guid}/archive")]
    public async Task<ServiceResponse<bool>> ArchiveQuestion(
        Guid questionId, CancellationToken ct)
        => await _mediator.Send(new ArchiveTriviaQuestionCommand(questionId), ct);

    /// <summary>
    /// POST /api/trivia/answer — Fan submits an answer; awards vote credits if correct.
    /// </summary>
    [HttpPost("answer")]
    public async Task<ServiceResponse<SubmitTriviaAnswerResult>> SubmitAnswer(
        [FromBody] SubmitTriviaAnswerCommand command, CancellationToken ct)
        => await _mediator.Send(command, ct);
}
