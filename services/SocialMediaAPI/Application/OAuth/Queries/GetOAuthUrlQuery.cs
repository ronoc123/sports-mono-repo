using Application.Common.Interfaces;
using Contracts.Contracts;
using MediatR;

namespace Application.OAuth.Queries;

public record GetOAuthUrlQuery(string ChannelId) : IRequest<ServiceResponse<OAuthUrlResponse>>;

public record OAuthUrlResponse(string Url);

public class GetOAuthUrlQueryHandler : IRequestHandler<GetOAuthUrlQuery, ServiceResponse<OAuthUrlResponse>>
{
    private readonly IYouTubeOAuthService _youTubeOAuth;

    public GetOAuthUrlQueryHandler(IYouTubeOAuthService youTubeOAuth)
    {
        _youTubeOAuth = youTubeOAuth;
    }

    public Task<ServiceResponse<OAuthUrlResponse>> Handle(
        GetOAuthUrlQuery request,
        CancellationToken cancellationToken)
    {
        var url = _youTubeOAuth.BuildAuthorizationUrl(request.ChannelId);
        return Task.FromResult(ServiceResponse.Ok(new OAuthUrlResponse(url)));
    }
}
