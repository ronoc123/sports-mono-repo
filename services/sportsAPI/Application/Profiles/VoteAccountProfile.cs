using Application.Dto.VoteAccount;
using AutoMapper;
using Domain.Shared_kernel;
using Domain.VoteAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Profiles
{
  public class VoteAccountMappingProfile : Profile
  {
    public VoteAccountMappingProfile()
    {
      // Map VoteAccount → VoteAccountDto
      CreateMap<VoteAccount, VoteAccountDto>()
        .ForMember(d => d.OrgId, opt => opt.MapFrom(s => s.OrgId.Value))
        .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId.Value))
        .ForMember(d => d.Transactions, opt => opt.MapFrom(s => s.Transactions));
    }
  }
}
