using Application.Dto.Organization;
using Application.Organizations.Queries.GetOrganizationDetails;
using AutoMapper;
using Domain.Organizations;
using Domain.ValueObjects.ConcreteTypes;

namespace Application.Profiles
{
  public class OrganizationMappingPorfile : Profile
  {
    public OrganizationMappingPorfile()
    {
      // Value object -> primitive conversions (optional, but nice to centralize)
      CreateMap<OrganizationId, Guid>()
        .ConvertUsing(src => src.Value);

      CreateMap<LeagueId, Guid>()
        .ConvertUsing(src => src.Value);

      // Main mapping: Organization -> OrganizationDto (list DTO)
      CreateMap<Organization, OrganizationDto>()
        // Ids (value objects -> Guid)
        .ForMember(d => d.Id,
          opt => opt.MapFrom(s => s.Id))
        .ForMember(d => d.LeagueId,
          opt => opt.MapFrom(s => s.LeagueId))

        // CreatedAt: nullable -> non-nullable
        .ForMember(d => d.CreatedAt,
          opt => opt.MapFrom(s => s.CreatedAt ?? DateTime.MinValue))

        // Venue
        .ForMember(d => d.Stadium,
          opt => opt.MapFrom(s => s.Venue.Stadium))
        .ForMember(d => d.Location,
          opt => opt.MapFrom(s => s.Venue.Location))
        .ForMember(d => d.StadiumCapacity,
          opt => opt.MapFrom(s => s.Venue.Capacity))

        // Media
        .ForMember(d => d.BadgeUrl,
          opt => opt.MapFrom(s => s.MediaAssets.BadgeUrl))
        .ForMember(d => d.LogoUrl,
          opt => opt.MapFrom(s => s.MediaAssets.LogoUrl))

        // Social
        .ForMember(d => d.Website,
          opt => opt.MapFrom(s => s.SocialLinks.Website))
        .ForMember(d => d.Facebook,
          opt => opt.MapFrom(s => s.SocialLinks.Facebook))
        .ForMember(d => d.Twitter,
          opt => opt.MapFrom(s => s.SocialLinks.Twitter))
        .ForMember(d => d.Instagram,
          opt => opt.MapFrom(s => s.SocialLinks.Instagram))

        // Team colors
        .ForMember(d => d.Color1,
          opt => opt.MapFrom(s => s.TeamColors.Color1))
        .ForMember(d => d.Color2,
          opt => opt.MapFrom(s => s.TeamColors.Color2))
        .ForMember(d => d.Color3,
          opt => opt.MapFrom(s => s.TeamColors.Color3));
    }

  }
}
