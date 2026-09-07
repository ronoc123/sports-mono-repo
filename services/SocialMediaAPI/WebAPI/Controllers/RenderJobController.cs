using Application.PostCycle.Dto;
using Application.RenderJobs.Commands;
using Application.RenderJobs.Dto;
using Application.RenderJobs.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/render-jobs")]
[AllowAnonymous] // TODO: switch to [Authorize] once auth is wired to this service
public class RenderJobController : ControllerBase
{
    private readonly IMediator _mediator;

    public RenderJobController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a reference image to Cloudflare R2.
    /// Returns the R2 object key to include in a CreateRenderJob request.
    /// </summary>
    [HttpPost("assets")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<ServiceResponse<UploadAssetResponse>>> UploadAsset(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(
            new UploadRenderAssetCommand(stream, file.ContentType, file.FileName),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a Pending render job. The VideoWorker will pick this up and execute it.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<CreateRenderJobResponse>>> Create(
        [FromBody] CreateRenderJobRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateRenderJobCommand(
                request.ChannelId,
                request.Prompt,
                request.Model,
                request.DurationSeconds,
                request.Resolution,
                request.AspectRatio,
                request.ReferenceImageKeys,
                request.KeyframeKeys,
                request.ModelOptions),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get the status of a render job.
    /// Status: Pending | Processing | Completed | Failed
    /// </summary>
    [HttpGet("{jobId}")]
    public async Task<ActionResult<ServiceResponse<RenderJobResponse>>> GetStatus(
        string jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRenderJobQuery(jobId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List all render jobs for a channel, newest first.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<RenderJobResponse>>>> ListByChannel(
        [FromQuery][Required] string channelId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListRenderJobsByChannelQuery(channelId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a 1-hour pre-signed URL for streaming the completed video from R2.
    /// </summary>
    [HttpGet("{jobId}/video-url")]
    public async Task<ActionResult<ServiceResponse<VideoUrlResponse>>> GetVideoUrl(
        string jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRenderJobVideoUrlQuery(jobId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a render job and all associated R2 assets.
    /// </summary>
    [HttpDelete("{jobId}")]
    public async Task<ActionResult<ServiceResponse<bool>>> Delete(
        string jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteRenderJobCommand(jobId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Start a post cycle from a completed local render job.
    /// Downloads the video from R2 and kicks off the posting pipeline.
    /// </summary>
    [HttpPost("{jobId}/post")]
    public async Task<ActionResult<ServiceResponse<StartPostCycleResponse>>> Post(
        string jobId,
        [FromBody] StartPostFromRenderJobRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StartPostCycleFromRenderJobCommand(
                jobId,
                request.ChannelId,
                request.Title,
                request.Description,
                request.Hashtags ?? new List<string>(),
                request.TargetPlatform),
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

public class StartPostFromRenderJobRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? Hashtags { get; set; }
    /// <summary>When set, only this platform is posted to. Null = post to all linked accounts.</summary>
    public string? TargetPlatform { get; set; }
}

public class CreateRenderJobRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Model { get; set; } = "ltx-2.3";
    public int DurationSeconds { get; set; } = 10;
    public string Resolution { get; set; } = "1280x720";
    public string AspectRatio { get; set; } = "16:9";
    public List<string> ReferenceImageKeys { get; set; } = new();
    public List<string>? KeyframeKeys { get; set; }
    public Dictionary<string, string>? ModelOptions { get; set; }
}
