using Application.Channel.Queries;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.VideoGeneration.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;
using SportifyCore.Domain;

namespace Application.VideoGeneration.Commands;

public record GenerateVideoCommand(
    string ChannelId,
    string VideoReference,
    string? UserPrompt)
    : IRequest<ServiceResponse<VideoGenerationResponse>>;

public class GenerateVideoCommandHandler
    : IRequestHandler<GenerateVideoCommand, ServiceResponse<VideoGenerationResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IPostRecordRepository _postRecordRepository;
    private readonly IVideoGenerationAdapter _videoGenerationAdapter;

    public GenerateVideoCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IPostRecordRepository postRecordRepository,
        IVideoGenerationAdapter videoGenerationAdapter)
    {
        _channelRepository = channelRepository;
        _postRecordRepository = postRecordRepository;
        _videoGenerationAdapter = videoGenerationAdapter;
    }

    public async Task<ServiceResponse<VideoGenerationResponse>> Handle(
        GenerateVideoCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        var recentRecords = await _postRecordRepository.GetRecentByChannelIdAsync(
            request.ChannelId, 5, cancellationToken);

        var generationRequest = new VideoGenerationRequest
        {
            ChannelId = request.ChannelId,
            ChannelName = channel.Name,
            StyleToneContext = channel.StyleToneContext,
            VideoReference = request.VideoReference,
            UserPrompt = request.UserPrompt,
            RecentHistory = recentRecords.Select(r => new PostHistoryItem
            {
                Title = r.Title,
                Description = r.Description,
                Hashtags = r.Hashtags,
                PostedAt = r.CreatedAt,
            }).ToList(),
        };

        var result = await _videoGenerationAdapter.GenerateAsync(generationRequest, cancellationToken);

        return ServiceResponse.Ok(new VideoGenerationResponse
        {
            Title = result.Title,
            Description = result.Description,
            Hashtags = result.Hashtags,
        });
    }
}
