
  namespace NotificationAPI.Email.Templates
  {
    public static class RewardEmailTemplate
    {
      public static string Render(string email, string claimToken, DateTime expiresAt)
      {
        var claimUrl = $"https://your-app.com/rewards/claim/{claimToken}";
        var expiresText = expiresAt.ToString("MMMM dd, yyyy");

        return $@"
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset=""UTF-8"">
          <title>You’ve received a reward</title>
        </head>
        <body style=""font-family: Arial, sans-serif; color: #333;"">

          <h2>🎉 You’ve received a reward!</h2>

          <p>Hello Conor!</p>
          <p>
            You’ve been awarded a reward. Click the button below to claim it before it expires.
          </p>

          <p style=""margin: 24px 0;"">
            <a href=""{claimUrl}""
               style=""background:#4f46e5;color:#fff;padding:12px 18px;
                      text-decoration:none;border-radius:6px;font-weight:bold;"">
              Claim your reward
            </ a >
          </ p >

          < p style = ""font - size: 0.9em; color: #666;"">
            This link expires on<strong>{ expiresText}</ strong >.
          </ p >

          < hr />

          < p style = ""font - size: 0.8em; color: #999;"">
            If you didn’t expect this email, you can safely ignore it.
          </ p >
        </ body >
        </ html > ";
       }
     }
 }

