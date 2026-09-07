using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VideoWorker.Abstractions;

namespace VideoWorker.Generators;

/// <summary>
/// Phase 1 generator: copies a sample MP4 to the output path.
/// Validates the full pipeline (MongoDB, R2, JobProcessor) without GPU hardware.
/// </summary>
public class StubVideoGenerator : IVideoGenerator
{
    private readonly string _sampleVideoPath;
    private readonly int _simulatedDelaySeconds;
    private readonly ILogger<StubVideoGenerator> _logger;

    public StubVideoGenerator(IConfiguration configuration, ILogger<StubVideoGenerator> logger)
    {
        _sampleVideoPath      = configuration["VideoGenerator:Stub:SampleVideoPath"] ?? "/app/samples/sample.mp4";
        _simulatedDelaySeconds = int.TryParse(configuration["VideoGenerator:Stub:SimulatedDelaySeconds"], out var d) ? d : 3;
        _logger = logger;
    }

    public async Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "StubVideoGenerator: simulating {Delay}s generation for model={Model}, duration={Duration}s",
            _simulatedDelaySeconds, request.Model, request.DurationSeconds);

        try
        {
            if (!File.Exists(_sampleVideoPath))
                return VideoGenerationResult.Failed(
                    $"Stub sample video not found at '{_sampleVideoPath}'. " +
                    "Set VideoGenerator:Stub:SampleVideoPath in configuration.");

            if (_simulatedDelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_simulatedDelaySeconds), cancellationToken);

            File.Copy(_sampleVideoPath, request.OutputPath, overwrite: true);

            _logger.LogInformation("StubVideoGenerator: wrote output to {OutputPath}", request.OutputPath);

            return VideoGenerationResult.Succeeded(request.OutputPath);
        }
        catch (OperationCanceledException)
        {
            return VideoGenerationResult.Failed("Generation was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StubVideoGenerator: unexpected error");
            return VideoGenerationResult.Failed(ex.Message);
        }
    }
}
