using Application.Organizations.Queries.GetOrganizationDetails;
using AutoMapper;
using Domain.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Profiles
{
  public class OrganizationMappingPorfile : Profile
  {
    public OrganizationMappingPorfile()
    {
      CreateMap<Organization, OrganizationDetailsDto>();
    }

  }
}
