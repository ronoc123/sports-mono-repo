using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ISocialMediaAdapter
{
    string Platform { get; }
    Task<PlatformPublishResult> PublishAsync(PublishRequest request, CancellationToken cancellationToken);
}
