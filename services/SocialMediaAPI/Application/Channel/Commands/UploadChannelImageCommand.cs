using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record UploadChannelImageCommand(
    string ChannelId,
    string ImagePath
) : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class UploadChannelImageCommandValidator : AbstractValidator<UploadChannelImageCommand>
{
    public UploadChannelImageCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.ImagePath).NotEmpty().WithMessage("Image path is required.");
    }
}

public class UploadChannelImageCommandHandler
    : IRequestHandler<UploadChannelImageCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public UploadChannelImageCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        UploadChannelImageCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        channel.CharacterImagePath = request.ImagePath;
        channel.LastModified = DateTime.UtcNow;

        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
