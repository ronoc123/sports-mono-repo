using Application.Dto.Player;
using Application.Dto.PlayerOption;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts.Contracts;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.PlayerOptions.Queries.GetAllPlayerOptions
{
    public class GetAllPlayerOptionsQueryHandler
        : IRequestHandler<GetAllPlayerOptionsQuery, ServiceResponse<List<PlayerOptionDto>>>
    {
        private readonly IRepository _repo;
        private readonly IMapper _mapper;

        public GetAllPlayerOptionsQueryHandler(IRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<List<PlayerOptionDto>>> Handle(GetAllPlayerOptionsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var query = _repo.Query<PlayerOption>();

            if (request.OrganizationId.HasValue)
            {
                query = query.Where(po => po.OrganizationId == OrganizationId.Of(request.OrganizationId.Value));
            }

            if (request.IsActive.HasValue)
            {
                query = request.IsActive.Value
                    ? query.Where(po => po.ExpiresAt > now)
                    : query.Where(po => po.ExpiresAt <= now);
            }

            if (request.IsExpired.HasValue)
            {
                query = request.IsExpired.Value
                    ? query.Where(po => po.ExpiresAt <= now)
                    : query.Where(po => po.ExpiresAt > now);
            }

            var dtoQuery = query.ProjectTo<PlayerOptionDto>(_mapper.ConfigurationProvider);
            var list = await dtoQuery.ToListAsync(cancellationToken);


            foreach (var po in list)
            {
                var player = await _repo.GetByIdAsync<Player, PlayerId>(
                    PlayerId.Of(po.PlayerId),
                    cancellationToken);

                if (player != null)
                    po.Player = _mapper.Map<PlayerDto>(player);
            }

            return ServiceResponse.Ok(list, "Player options retrieved.");

        }
    }
}
