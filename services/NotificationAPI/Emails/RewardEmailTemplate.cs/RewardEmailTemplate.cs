
namespace NotificationAPI.Email.Templates
{
    public static class RewardEmailTemplate
    {
        public static string Render(string email, DateTime expiresAt)
        {
            var claimUrl = $"https://your-app.com/rewards/claim";
            var expiresText = expiresAt.ToString("MMMM dd, yyyy");

            return $@"
        <!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  </head>
  <body
    style=""font-family: Arial, sans-serif; color: #333; margin: 0; padding: 0""
  >
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
      <tr>
        <td align=""center"" style=""padding: 24px"">
          <table
            width=""600""
            cellpadding=""0""
            cellspacing=""0""
            style=""max-width: 600px""
          >
            <tr>
              <td>
                <h2 style=""margin-top: 0"">🎉 You’ve received a gift! From the [ORG NAME]</h2>

                <p>Hello Conor!</p>

                <p>
                  You’ve been gifted a [POINTS].
                </p>

                <p style=""margin: 24px 0"">
                  <a
                    href=""http://localhost:4200/""
                    style=""
                      background-color: #4f46e5;
                      color: #ffffff;
                      padding: 12px 18px;
                      text-decoration: none;
                      border-radius: 6px;
                      font-weight: bold;
                      display: inline-block;
                    ""
                  >
                    Login here to spend your points
                  </a>
                </p>
                <hr
                  style=""
                    border: none;
                    border-top: 1px solid #e5e7eb;
                    margin: 24px 0;
                  ""
                />

                <p style=""font-size: 0.8em; color: #999"">
                  If you didn’t expect this email, you can safely ignore it.
                </p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>
 ";
        }
    }
}

