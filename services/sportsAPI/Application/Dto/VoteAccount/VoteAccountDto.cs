using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.VoteAccount
{
  public class VoteAccountDto
  {
    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }

    public long Balance { get; set; }
    public long Version { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<VoteTransactionDto> Transactions { get; set; } = new();
  }
}
