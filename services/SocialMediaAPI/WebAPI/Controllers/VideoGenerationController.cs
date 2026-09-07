using Application.VideoGeneration.Commands;
using Application.VideoGeneration.Dto;
using Application.VideoGeneration.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/video-generation")]
[AllowAnonymous]
public class VideoGenerationController : ControllerBase
{
    private readonly IMediator _mediator;

    public VideoGenerationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Start an async AI video generation job using the channel's stored character image.
    /// Returns a jobId to poll for status.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<ServiceResponse<StartVideoGenerationResponse>>> Start(
        [FromForm] StartVideoGenerationFormRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StartVideoGenerationCommand(
                request.ChannelId,
                request.PromptOverride,
                request.TargetDurationSeconds ?? 15),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Poll the status of a video generation job.
    /// Status: Queued | Generating | Ready | Failed | TimedOut | Consumed
    /// </summary>
    [HttpGet("{jobId}")]
    public async Task<ActionResult<ServiceResponse<VideoGenerationJobResponse>>> GetStatus(
        string jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVideoGenerationJobQuery(jobId), cancellationToken);
        return Ok(result);
    }
}

public class StartVideoGenerationFormRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string? PromptOverride { get; set; }
    public int? TargetDurationSeconds { get; set; }
}
