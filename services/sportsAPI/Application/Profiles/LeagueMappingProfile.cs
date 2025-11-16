using Application.Dto.League;
using AutoMapper;
using Domain.Leagues;

namespace Application.Profiles
{
  public class LeagueMappingProfile : Profile
  {
    public LeagueMappingProfile()
    {
      CreateMap<League, LeagueDto>();
    }
  }
}
