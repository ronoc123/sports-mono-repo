using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialMediaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ISocialMediaDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ISocialMediaDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "healthy", service = "SocialMediaAPI" });

    [HttpGet("db")]
    public async Task<IActionResult> CheckDb()
    {
        try
        {
            var collection = _dbContext.GetCollection<BsonDocument>("_health");
            var command = new BsonDocumentCommand<BsonDocument>(new BsonDocument("ping", 1));
            await collection.Database.RunCommandAsync(command);
            return Ok(new { status = "connected", database = "MongoDB" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            return StatusCode(503, new { status = "unavailable", error = ex.Message });
        }
    }
}
