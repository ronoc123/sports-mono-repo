using Application.Dto.VoteAccount;
using Application.Organizations.Queries.GetOrganizationDetails;
using Contracts.Contracts;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class VoteAccount : ControllerBase
  {
    // GET: api/<VoteAccount>
    [HttpGet]
    public IEnumerable<string> Get()
    {
      return new string[] { "value1", "value2" };
    }

    // GET api/<VoteAccount>/5
    [HttpGet("get-vote-account/{userId}")]
    public async Task<ServiceResponse<VoteAccountDto>> GetAll([FromQuery] Guid userId)
    {
      return null;
    }

    [HttpGet("get-vote-account/{userId}/organization/{organizationId}")]
    public async Task<ServiceResponse<VoteAccountDto>> Get([FromQuery] Guid userId, [FromQuery] Guid organizationId)
    {
      return null;
    }

    // POST api/<VoteAccount>
    [HttpPost]
    public void Redeem([FromBody] string value)
    {
    }

  }
}
