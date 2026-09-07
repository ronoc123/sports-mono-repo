using Application.Channel.Dto;
using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record LinkYouTubeAccountCommand(string ChannelId, string AuthCode)
    : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class LinkYouTubeAccountCommandValidator : AbstractValidator<LinkYouTubeAccountCommand>
{
    public LinkYouTubeAccountCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.AuthCode).NotEmpty().WithMessage("Authorization code is required.");
    }
}

public class LinkYouTubeAccountCommandHandler
    : IRequestHandler<LinkYouTubeAccountCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IYouTubeOAuthService _youTubeOAuth;
    private readonly IEncryptionService _encryption;

    public LinkYouTubeAccountCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IYouTubeOAuthService youTubeOAuth,
        IEncryptionService encryption)
    {
        _channelRepository = channelRepository;
        _youTubeOAuth = youTubeOAuth;
        _encryption = encryption;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        LinkYouTubeAccountCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        var (accessToken, refreshToken) = await _youTubeOAuth.ExchangeCodeAsync(request.AuthCode);
        var channelTitle = await _youTubeOAuth.GetChannelTitleAsync(accessToken);
        var encryptedToken = _encryption.Encrypt(refreshToken, out var iv);

        var linkedAccount = new global::Domain.Channel.LinkedAccount
        {
            Platform = "YouTube",
            AccountDisplayName = channelTitle,
            EncryptedRefreshToken = encryptedToken,
            TokenIv = iv,
            LinkedAt = DateTime.UtcNow,
            TokenStatus = "active",
        };

        channel.AddLinkedAccount(linkedAccount);
        channel.LastModified = DateTime.UtcNow;
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
