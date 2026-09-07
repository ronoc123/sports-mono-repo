using Application.RenderJobs.Commands;
using Application.RenderJobs.Dto;
using Application.RenderJobs.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Poll the status of a render job.
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
