using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record UploadChannelAudioCommand(
    string ChannelId,
    string AudioPath
) : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class UploadChannelAudioCommandValidator : AbstractValidator<UploadChannelAudioCommand>
{
    public UploadChannelAudioCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.AudioPath).NotEmpty().WithMessage("Audio path is required.");
    }
}

public class UploadChannelAudioCommandHandler
    : IRequestHandler<UploadChannelAudioCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public UploadChannelAudioCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        UploadChannelAudioCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        channel.ContextAudioPath = request.AudioPath;
        channel.LastModified = DateTime.UtcNow;

        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
