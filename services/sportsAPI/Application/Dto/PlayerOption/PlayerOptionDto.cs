using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.PlayerOption
{
  public class PlayerOptionDto
  {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Votes { get; set; }
    public Guid PlayerId { get; set; }
    public Guid OrganizationId { get; set; }

    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsPopular { get; set; }
    public bool IsTrending { get; set; }
    public int DaysRemaining { get; set; }
  }
}
