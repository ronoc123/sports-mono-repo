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
    /// Each clip is generated separately and concatenated into one output video.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ServiceResponse<CreateRenderJobResponse>>> Create(
        [FromBody] CreateRenderJobRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateRenderJobCommand(
                request.ChannelId,
                request.Model,
                request.DurationSeconds,
                request.Resolution,
                request.AspectRatio,
                request.Clips,
                request.ModelOptions,
                request.UseChannelImage),
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
    /// Uses Claude to generate a video concept (prompt + 6 keyframe scenes) from a rough idea.
    /// Passes the channel's character image and context automatically.
    /// The result is intended to auto-fill the local video generation form.
    /// </summary>
    [HttpPost("ideate")]
    public async Task<ActionResult<ServiceResponse<IdeateVideoResponse>>> Ideate(
        [FromBody] IdeateVideoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new IdeateVideoQuery(request.ChannelId, request.UserIdea),
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

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

public class IdeateVideoRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string UserIdea { get; set; } = string.Empty;
}

public class CreateRenderJobRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string Model { get; set; } = "h3-fl2va";
    public int DurationSeconds { get; set; } = 4;
    public string Resolution { get; set; } = "480x832";
    public string AspectRatio { get; set; } = "9:16";
    public List<CreateRenderJobClipRequest> Clips { get; set; } = new();
    public Dictionary<string, string>? ModelOptions { get; set; }
    /// <summary>
    /// When false the backend will NOT auto-upload the channel's character image as the
    /// default start frame for clips that have no explicit StartImageKey. Defaults to true.
    /// </summary>
    public bool UseChannelImage { get; set; } = true;
}
