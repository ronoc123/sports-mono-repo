using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dto.Player
{
  public class PlayerDto
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid LeagueId { get; set; }
    public Guid? OrganizationId { get; set; }

    // Business logic properties
    public bool IsActive { get; set; }
    public bool IsVeteran { get; set; }
    public bool IsYoungPlayer { get; set; }
    public decimal MarketValue { get; set; }

    // Related data
    public string? LeagueName { get; set; }
    public string? OrganizationName { get; set; }
    public int ActivePlayerOptionsCount { get; set; }
  }
}
