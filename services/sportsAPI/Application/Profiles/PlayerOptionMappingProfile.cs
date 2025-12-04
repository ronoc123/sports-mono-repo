using Application.Dto.PlayerOption;
using AutoMapper;
using Domain.PlayerOption;
using Domain.ValueObjects.ConcreteTypes;

namespace Application.Profiles
{
  public class PlayerOptionMappingProfile : Profile
  {
    public PlayerOptionMappingProfile()
    {
      //
      // VALUE OBJECT → PRIMITIVE CONVERSIONS
      //
      CreateMap<PlayerOptionId, Guid>()
          .ConvertUsing(src => src.Value);

      CreateMap<PlayerId, Guid>()
          .ConvertUsing(src => src.Value);

      CreateMap<OrganizationId, Guid>()
          .ConvertUsing(src => src.Value);

      //
      // MAIN MAPPING: DOMAIN → DTO
      //
      CreateMap<PlayerOption, PlayerOptionDto>()
          // Id(s)
          .ForMember(d => d.Id,
              opt => opt.MapFrom(s => s.Id))

          .ForMember(d => d.PlayerId,
              opt => opt.MapFrom(s => s.PlayerId))

          .ForMember(d => d.OrganizationId,
              opt => opt.MapFrom(s => s.OrganizationId))

          // Primitive props
          .ForMember(d => d.Title,
              opt => opt.MapFrom(s => s.Title))

          .ForMember(d => d.Description,
              opt => opt.MapFrom(s => s.Description))

          .ForMember(d => d.Votes,
              opt => opt.MapFrom(s => s.Votes))

          // Business logic flags
          .ForMember(d => d.IsActive,
              opt => opt.MapFrom(s => s.IsActive))

          .ForMember(d => d.IsExpired,
              opt => opt.MapFrom(s => s.IsExpired))

          .ForMember(d => d.IsPopular,
              opt => opt.MapFrom(s => s.IsPopular))

          .ForMember(d => d.IsTrending,
              opt => opt.MapFrom(s => s.IsTrending))

          .ForMember(d => d.DaysRemaining,
              opt => opt.MapFrom(s => s.DaysRemaining));

    }
  }
}
