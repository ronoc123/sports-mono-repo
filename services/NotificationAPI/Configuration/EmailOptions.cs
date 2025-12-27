using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationAPI.Configuration
{
  public class EmailOptions
  {
    public string SmtpHost { get; init; } = default!;
    public int SmtpPort { get; init; }
    public bool EnableSsl { get; init; }
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FromEmail { get; init; } = default!;
    public string FromName { get; init; } = default!;
  }
}
