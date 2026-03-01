using MediatR;

namespace Application.H2H.Commands.ResolveMatch;

public record ResolveMatchCommand(Guid MatchId) : IRequest;
