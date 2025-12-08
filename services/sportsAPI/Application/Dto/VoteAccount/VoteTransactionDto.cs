using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.VoteAccount
{
  public class VoteTransactionDto
  {
    public long Id { get; set; }

    public Guid OrgId { get; set; }
    public Guid UserId { get; set; }

    public long Amount { get; set; }
    public string Reason { get; set; } = default!;
    public string? RefId { get; set; }
    public Guid? PlayerOptionId { get; set; }
    public string? SpendId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
  }
}
