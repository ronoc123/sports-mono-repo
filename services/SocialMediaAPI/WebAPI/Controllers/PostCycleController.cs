using Application.PostCycle.Commands;
using Application.PostCycle.Dto;
using Application.PostCycle.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/post-cycle")]
[AllowAnonymous]
public class PostCycleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public PostCycleController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("start")]
    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
    public async Task<ActionResult<ServiceResponse<StartPostCycleResponse>>> Start(
        [FromForm] StartPostCycleFormRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(ServiceResponse.Fail<StartPostCycleResponse>("No video file provided."));

        var tempDir = _configuration["TempStorage:Path"] ?? "temp-uploads";
        Directory.CreateDirectory(tempDir);
        var ext = Path.GetExtension(request.File.FileName);
        var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid()}{ext}");

        await using (var stream = System.IO.File.Create(tempPath))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var hashtags = string.IsNullOrWhiteSpace(request.Hashtags)
            ? new List<string>()
            : request.Hashtags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        var result = await _mediator.Send(
            new StartPostCycleCommand(
                request.ChannelId,
                tempPath,
                request.Title,
                request.Description,
                hashtags),
            cancellationToken);

        return CreatedAtAction(nameof(GetPostCycle), new { jobId = result.Data!.JobId }, result);
    }

    [HttpGet("{jobId}")]
    public async Task<ActionResult<ServiceResponse<PostCycleJobResponse>>> GetPostCycle(
        string jobId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPostCycleQuery(jobId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{jobId}/retry/{platform}")]
    public async Task<ActionResult<ServiceResponse<PostCycleJobResponse>>> RetryPlatform(
        string jobId,
        string platform,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RetryPlatformCommand(jobId, platform), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Start a post cycle using a completed AI-generated video (status: Ready).
    /// The generation job is marked Consumed; existing polling applies to the returned jobId.
    /// </summary>
    [HttpPost("start-from-generation")]
    public async Task<ActionResult<ServiceResponse<StartPostCycleResponse>>> StartFromGeneration(
        [FromBody] StartPostCycleFromGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var hashtags = request.Hashtags ?? new List<string>();

        var result = await _mediator.Send(
            new StartPostCycleFromGenerationCommand(
                request.ChannelId,
                request.VideoGenerationJobId,
                request.Title,
                request.Description,
                hashtags),
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetPostCycle), new { jobId = result.Data!.JobId }, result);
    }
}

public class StartPostCycleFormRequest
{
    [Microsoft.AspNetCore.Mvc.ModelBinder]
    public IFormFile? File { get; set; }
    public string ChannelId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Hashtags { get; set; } // comma-separated
}

public class StartPostCycleFromGenerationRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string VideoGenerationJobId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string>? Hashtags { get; set; }
}
