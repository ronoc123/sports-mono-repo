using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;
using SportifyCore.Domain;

namespace Application.RenderJobs.Queries;

public record IdeateVideoQuery(string ChannelId, string UserIdea)
    : IRequest<ServiceResponse<IdeateVideoResponse>>;

public class IdeateVideoQueryHandler
    : IRequestHandler<IdeateVideoQuery, ServiceResponse<IdeateVideoResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IClaudeIdeationService _ideationService;

    public IdeateVideoQueryHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IClaudeIdeationService ideationService)
    {
        _channelRepository = channelRepository;
        _ideationService = ideationService;
    }

    public async Task<ServiceResponse<IdeateVideoResponse>> Handle(
        IdeateVideoQuery request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        var ideationRequest = new IdeationRequest(
            ChannelName: channel.Name,
            StyleToneContext: channel.StyleToneContext,
            PromptTemplate: channel.PromptTemplate,
            CharacterImagePath: channel.CharacterImagePath,
            UserIdea: request.UserIdea);

        var result = await _ideationService.IdeateAsync(ideationRequest, cancellationToken);

        if (result is null)
            return ServiceResponse.Fail<IdeateVideoResponse>(
                "Claude could not generate a video concept. Please try again.");

        return ServiceResponse.Ok(new IdeateVideoResponse
        {
            Prompt = result.Prompt,
            Scenes = result.Scenes,
        });
    }
}
