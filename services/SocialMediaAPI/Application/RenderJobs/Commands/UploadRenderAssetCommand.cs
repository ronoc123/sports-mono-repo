using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using Contracts.Contracts;
using FluentValidation;
using MediatR;

namespace Application.RenderJobs.Commands;

public record UploadRenderAssetCommand(
    Stream FileStream,
    string ContentType,
    string FileName
) : IRequest<ServiceResponse<UploadAssetResponse>>;

public class UploadRenderAssetCommandValidator : AbstractValidator<UploadRenderAssetCommand>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public UploadRenderAssetCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, and WebP images are accepted.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");
    }
}

public class UploadRenderAssetCommandHandler
    : IRequestHandler<UploadRenderAssetCommand, ServiceResponse<UploadAssetResponse>>
{
    private readonly IR2StorageService _r2;

    public UploadRenderAssetCommandHandler(IR2StorageService r2)
    {
        _r2 = r2;
    }

    public async Task<ServiceResponse<UploadAssetResponse>> Handle(
        UploadRenderAssetCommand request,
        CancellationToken cancellationToken)
    {
        var ext       = Path.GetExtension(request.FileName).ToLowerInvariant();
        var objectKey = $"generation/assets/{Guid.NewGuid()}{ext}";

        await _r2.UploadAsync(objectKey, request.FileStream, request.ContentType, cancellationToken);

        return ServiceResponse.Ok(new UploadAssetResponse { ObjectKey = objectKey });
    }
}
