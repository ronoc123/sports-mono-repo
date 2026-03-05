using Application.Common.Interfaces;
using Contracts.Contracts;
using Domain.DomainServices.VotingService.VotingService;
using Domain.PlayerOption;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.PlayerOptions.Commands.VotePlayerOption;

public class VotePlayerOptionCommandHandler
    : IRequestHandler<VotePlayerOptionCommand, ServiceResponse<bool>>
{
    private readonly IRepository _repo;
    private readonly IDomainVotingService _votingService;
    private readonly IBalanceNotificationService _balanceNotifier;

    public VotePlayerOptionCommandHandler(
        IRepository repo,
        IDomainVotingService votingService,
        IBalanceNotificationService balanceNotifier)
    {
        _repo = repo;
        _votingService = votingService;
        _balanceNotifier = balanceNotifier;
    }

    public async Task<ServiceResponse<bool>> Handle(VotePlayerOptionCommand request, CancellationToken ct)
    {
        var optionId = PlayerOptionId.Of(request.PlayerOptionId);

        var option = await _repo.GetByIdAsync<PlayerOption, PlayerOptionId>(optionId, ct);
        if (option is null)
            throw new ValidationException("Player option not found");

        var userId = UserId.Of(request.UserId);
        var leagueId = LeagueId.Of(request.LeagueId);

        var account = await _repo.GetByIdAsync<VoteAccount>(ct, leagueId, userId);

        if (account is null)
            throw new ValidationException("Insufficient Funds.");

        _votingService.Vote(account, option, userId, request.VoteAmount);

        await _repo.SaveChangesAsync(ct);

        _ = _balanceNotifier.NotifyBalanceChangedAsync(userId.Value.ToString(), account.Balance, ct);

        return ServiceResponse.Ok(true, "Vote applied successfully.");
    }
}
