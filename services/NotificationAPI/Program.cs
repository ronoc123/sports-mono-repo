using BuildingBlocks.Messageing.MassTransit;
using NotificationAPI.Configuration;
using NotificationAPI.Consumers;
using NotificationAPI.Email;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMessageBroker(
    builder.Configuration,
    assembly: typeof(RewardEmailRequestedConsumer).Assembly
);

// 🔹 Email configuration from appsettings.json
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email")
);

// 🔹 Email sender implementation
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();



var host = builder.Build();
host.Run();
