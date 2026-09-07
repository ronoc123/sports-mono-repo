using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record UpdatePromptTemplateCommand(string ChannelId, string? PromptTemplate)
    : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class UpdatePromptTemplateCommandValidator : AbstractValidator<UpdatePromptTemplateCommand>
{
    public UpdatePromptTemplateCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.PromptTemplate)
            .MaximumLength(2000).WithMessage("Prompt template must not exceed 2000 characters.")
            .When(x => x.PromptTemplate is not null);
    }
}

public class UpdatePromptTemplateCommandHandler
    : IRequestHandler<UpdatePromptTemplateCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public UpdatePromptTemplateCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        UpdatePromptTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        channel.PromptTemplate = string.IsNullOrWhiteSpace(request.PromptTemplate)
            ? null
            : request.PromptTemplate.Trim();
        channel.LastModified = DateTime.UtcNow;

        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
