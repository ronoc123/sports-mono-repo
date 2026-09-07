using Application.PostRecord.Dto;
using Application.PostRecord.Queries;
using Contracts.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/post-records")]
[AllowAnonymous]
public class PostRecordController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostRecordController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<ServiceResponse<MetadataSuggestionsResponse>>> GetSuggestions(
        [FromQuery] string channelId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMetadataSuggestionsQuery(channelId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<ServiceResponse<PostHistoryPageResponse>>> GetHistory(
        [FromQuery] string channelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetPostHistoryQuery(channelId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<PostRecordDetailResponse>>> GetPostRecord(
        string id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPostRecordQuery(id), cancellationToken);
        return Ok(result);
    }
}
