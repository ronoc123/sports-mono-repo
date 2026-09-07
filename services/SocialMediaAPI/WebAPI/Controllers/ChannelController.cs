using Application.Channel.Commands;
using Application.Channel.Dto;
using Application.Channel.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.VideoGeneration.Commands;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/channels")]
[AllowAnonymous]
public class ChannelController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public ChannelController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResponse<List<ChannelSummaryResponse>>>> GetChannels(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChannelsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> GetChannel(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChannelQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> CreateChannel(
        [FromBody] CreateChannelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateChannelCommand(request.Name, request.Description, request.StyleToneContext),
            cancellationToken);
        return CreatedAtAction(nameof(GetChannel), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> UpdateChannel(
        string id,
        [FromBody] UpdateChannelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateChannelCommand(id, request.Name, request.Description, request.StyleToneContext),
            cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServiceResponse<bool>>> DeleteChannel(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteChannelCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}/accounts/{platform}")]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> UnlinkAccount(
        string id,
        string platform,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UnlinkAccountCommand(id, platform), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id}/prompt-template")]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> UpdatePromptTemplate(
        string id,
        [FromBody] UpdatePromptTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdatePromptTemplateCommand(id, request.PromptTemplate),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id}/image")]
    [RequestSizeLimit(20_971_520)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20_971_520)]
    public async Task<ActionResult<ServiceResponse<ChannelDetailResponse>>> UploadChannelImage(
        string id,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
            return BadRequest(ServiceResponse.Fail<ChannelDetailResponse>("No image provided."));

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return BadRequest(ServiceResponse.Fail<ChannelDetailResponse>("Image must be JPG, PNG, or WebP."));

        var imageDir = _configuration["CharacterImages:Path"] ?? "char-images";
        Directory.CreateDirectory(imageDir);

        // Overwrite previous image for this channel (one image per channel)
        var imagePath = Path.Combine(imageDir, $"{id}{ext}");

        // Delete any prior image with a different extension
        foreach (var old in Directory.GetFiles(imageDir, $"{id}.*"))
            System.IO.File.Delete(old);

        await using (var stream = System.IO.File.Create(imagePath))
        {
            await image.CopyToAsync(stream, cancellationToken);
        }

        var result = await _mediator.Send(
            new UploadChannelImageCommand(id, imagePath),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id}/image")]
    public IActionResult GetChannelImage(string id)
    {
        var charImagesDir = _configuration["CharacterImages:Path"] ?? "char-images";
        if (!Directory.Exists(charImagesDir))
            return NotFound();

        var file = Directory.GetFiles(charImagesDir, $"{id}.*").FirstOrDefault();
        if (file is null)
            return NotFound();

        var contentType = Path.GetExtension(file).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        return PhysicalFile(Path.GetFullPath(file), contentType);
    }
}

public record CreateChannelRequest(string Name, string Description, string StyleToneContext);
public record UpdateChannelRequest(string Name, string Description, string StyleToneContext);
public record UpdatePromptTemplateRequest(string? PromptTemplate);
