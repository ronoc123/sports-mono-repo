using Application.Channel.Commands;
using Application.OAuth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/oauth")]
public class OAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public OAuthController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns the Google OAuth authorization URL. Opens in a browser popup.
    /// </summary>
    [HttpGet("youtube/authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> GetYouTubeAuthUrl(
        [FromQuery] string channelId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOAuthUrlQuery(channelId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Receives the OAuth callback from Google, exchanges the code for tokens,
    /// encrypts and stores the refresh token, then closes the popup.
    /// AllowAnonymous — this endpoint is called by Google's redirect, not by the app frontend.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<ContentResult> HandleCallback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new LinkYouTubeAccountCommand(state, code), cancellationToken);

        return Content("""
            <!DOCTYPE html>
            <html>
            <head><title>Account Linked</title></head>
            <body>
            <p>YouTube account linked successfully. This window will close automatically.</p>
            <script>
              if (window.opener) { window.opener.postMessage('oauth-success', '*'); }
              setTimeout(function() { window.close(); }, 1500);
            </script>
            </body>
            </html>
            """, "text/html");
    }
}
