using Contracts.Contracts;
using MediatR;

namespace Application.Poll.Commands.ArchivePoll;

public record ArchivePollCommand(Guid PollId) : IRequest<ServiceResponse<bool>>;
