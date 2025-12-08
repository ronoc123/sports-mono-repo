using Application.Dto.Player;
using AutoMapper;
using Domain.Player;
using Domain.ValueObjects.ConcreteTypes;

namespace Application.Profiles
{
  public class PlayerMappingProfile : Profile
  {
    public PlayerMappingProfile()
    {
      // Value object -> Guid conversions
      CreateMap<PlayerId, Guid>()
          .ConvertUsing(src => src.Value);

      CreateMap<LeagueId, Guid>()
          .ConvertUsing(src => src.Value);

      CreateMap<OrganizationId, Guid?>()
          .ConvertUsing(src => src.Value);

      // Domain -> DTO
      CreateMap<Player, PlayerDto>()
          .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
          .ForMember(d => d.LeagueId, opt => opt.MapFrom(s => s.LeagueId))
          .ForMember(d => d.OrganizationId, opt => opt.MapFrom(s => s.OrganizationId))
          .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
          .ForMember(d => d.Position, opt => opt.MapFrom(s => s.Position))
          .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
          .ForMember(d => d.Age, opt => opt.MapFrom(s => s.Age))
          .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
          .ForMember(d => d.IsVeteran, opt => opt.MapFrom(s => s.IsVeteran))
          .ForMember(d => d.IsYoungPlayer, opt => opt.MapFrom(s => s.IsYoungPlayer));
    }
  }
}
