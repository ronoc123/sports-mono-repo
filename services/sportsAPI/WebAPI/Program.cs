using Application;
using BuildingBlocks.Messageing.MassTransit;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Web;
using Web.Web;
using Application.Common.Interfaces;
using Application.Marketplace.Services;
using Infrastructure.Marketplace.Services;
using sportsAPI.Hubs;
using sportsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSportifyWeb();
builder.Services.AddMessageBroker(builder.Configuration, typeof(RewardReveivedConsumer).Assembly);

builder.Services.AddCors(options =>
{
    // AllowAnyOrigin() cannot be combined with AllowCredentials(), which SignalR requires.
    // SetIsOriginAllowed(_ => true) is the dev-safe equivalent that supports credentials.
    options.AddPolicy("AllowAll", policy =>
    {
        policy
          .SetIsOriginAllowed(_ => true)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
    });

    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
          .WithOrigins(
              "http://localhost:4200",
              "http://localhost:3000",
              "http://localhost:5173",
              "http://localhost:8080",
              "https://localhost:4200",
              "https://localhost:3000",
              "https://localhost:5173",
              "https://localhost:8080"
          )
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
    });
});

// Add JWT Authentication - configured to validate tokens from Identity Service
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
    // Allow SignalR to receive JWT via ?access_token= query string
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("GMOnly", policy => policy.RequireRole("GM"));
    options.AddPolicy("CSPOnly", policy => policy.RequireRole("CSP"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});


// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "sportsAPI", Version = "v1" });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Please enter the JWT token with Bearer prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// SignalR (real-time auction hub)
builder.Services.AddSignalR();

// Auction real-time notification service (injected into command handlers)
builder.Services.AddScoped<IAuctionSignalRService, AuctionSignalRService>();

// Balance real-time notification service
builder.Services.AddScoped<IBalanceNotificationService, BalanceNotificationService>();

// Auction expiry background service (Story 3.3)
builder.Services.AddHostedService<AuctionExpiryService>();

// Register additional services
builder.Services.AddHttpClient();


builder.Services.AddHttpClient<SportsDbImporter>(c =>
{
    c.BaseAddress = new Uri("https://www.thesportsdb.com/api/v1/json/123/");
});


var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");  // 🔥 100% fixes your Docker + Visual Studio port chaos
}
else
{
    app.UseCors("AllowFrontend");  // restrictive prod policy
}


// Exclude the Stripe webhook endpoint from HTTPS redirect —
// the Stripe CLI forwards over HTTP and does not follow 307 redirects.
app.UseWhen(
    ctx => !ctx.Request.Path.StartsWithSegments("/api/store/webhook"),
    a => a.UseHttpsRedirection());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction").RequireCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend");
app.MapHub<BalanceHub>("/hubs/balance").RequireCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend");

// Database seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SportsDbAppContext>();
        var importer = scope.ServiceProvider.GetRequiredService<SportsDbImporter>();

        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting database seeding...");
        //await SeedData.SeedAsync(context);
        await SeedData.SeedAsync(context, importer);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var db = services.GetRequiredService<SportsDbAppContext>();

        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database migration failed.");
        throw; // 🚨 fail fast — app should NOT start with broken schema
    }
}


app.Run();

