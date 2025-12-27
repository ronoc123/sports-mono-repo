using Microsoft.Extensions.Options;
using NotificationAPI.Configuration;
using System.Net;
using System.Net.Mail;

namespace NotificationAPI.Email
{
  public class SmtpEmailSender : IEmailSender
  {
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
      _options = options.Value;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
      var message = new MailMessage
      {
        From = new MailAddress(_options.FromEmail, _options.FromName),
        Subject = subject,
        Body = htmlBody,
        IsBodyHtml = true
      };

      message.To.Add(to);

      // ✅ THIS IS WHERE YOUR CODE GOES
      using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
      {
        EnableSsl = _options.EnableSsl
      };

      // Only set credentials if they exist (smtp4dev doesn't need them)
      if (!string.IsNullOrWhiteSpace(_options.Username))
      {
        client.Credentials = new NetworkCredential(
            _options.Username,
            _options.Password
        );
      }

      await client.SendMailAsync(message);
    }
  }
}
