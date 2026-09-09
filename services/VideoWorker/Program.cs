using Serilog;
using VideoWorker;
using VideoWorker.Abstractions;
using VideoWorker.Generators;
using VideoWorker.Infrastructure;

// Load .env file from the services/ directory (two levels up from VideoWorker/).
// Safe to call even if the file doesn't exist (e.g. on the VM using OS env vars).
DotNetEnv.Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);

// HttpClient factory — used by WanGpVideoGenerator
builder.Services.AddHttpClient("WanGp");

builder.Services.AddSerilog(lc => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

// Infrastructure services
builder.Services.AddSingleton<MongoJobQueue>();
builder.Services.AddSingleton<R2AssetService>();
builder.Services.AddSingleton<TempFileManager>();

// Video generator — switch via VideoGenerator:Type config
var generatorType = builder.Configuration["VideoGenerator:Type"] ?? "Stub";
if (generatorType.Equals("WanGP", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IVideoGenerator, WanGpVideoGenerator>();
}
else
{
    builder.Services.AddSingleton<IVideoGenerator, StubVideoGenerator>();
}

// Job processor and worker
builder.Services.AddSingleton<JobProcessor>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
