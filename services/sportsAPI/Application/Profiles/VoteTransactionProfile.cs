using Application.Dto.VoteAccount;
using AutoMapper;
using Domain.Shared_kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Profiles
{
  internal class VoteTransactionProfile : Profile
  {
    public VoteTransactionProfile()
    {

      // Map VoteTransaction → VoteTransactionDto
      CreateMap<VoteTransaction, VoteTransactionDto>()
        .ForMember(d => d.LeagueId, opt => opt.MapFrom(s => s.LeagueId.Value))
        .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId.Value))
        .ForMember(d => d.PlayerOptionId,
          opt => opt.MapFrom(s => s.PlayerOptionId != null ? s.PlayerOptionId.Value : (Guid?)null)
        );
    }
  }
}
