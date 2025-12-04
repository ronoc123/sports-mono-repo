using Contracts.Contracts;
using Domain.PlayerOption;
using Domain.VoteAccount;
using Domain.ValueObjects.ConcreteTypes;
using Domain.Repositories;
using MediatR;

namespace Application.PlayerOptions.Commands.VotePlayerOption;

public class VotePlayerOptionCommandHandler
    : IRequestHandler<VotePlayerOptionCommand, ServiceResponse<bool>>
{
  private readonly IRepository _repo;

  public VotePlayerOptionCommandHandler(IRepository repo)
  {
    _repo = repo;
  }

  public async Task<ServiceResponse<bool>> Handle(VotePlayerOptionCommand request, CancellationToken ct)
  {
    var optionId = PlayerOptionId.Of(request.PlayerOptionId);

    var option = await _repo.GetByIdAsync<PlayerOption, PlayerOptionId>(optionId, ct);
    if (option is null)
      return ServiceResponse.Fail<bool>("Player option not found");

    var userId = UserId.Of(request.UserId);
    var orgId = option.OrganizationId;

    var account = await _repo.GetByIdAsync<VoteAccount>(ct, orgId, userId);

    if (account is null)
      return ServiceResponse.Fail<bool>("VoteAccount does not exist for this user/org");

    string spendId = Guid.NewGuid().ToString();
    var token = account.AuthorizeSpend(optionId, request.VoteAmount, spendId);

    option.CastVote(userId, token);

    await _repo.SaveChangesAsync(ct);

    return ServiceResponse.Ok(true, "Vote applied successfully.");
  }
}
