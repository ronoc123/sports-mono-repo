using Application.Dto.Player;
using AutoMapper;
using Domain.Player;


namespace Application.Profiles
{
  public class PlayerMappingProfile : Profile
  {
    public PlayerMappingProfile()
    {
      CreateMap<Player, PlayerDto>();
    }
  }
}
