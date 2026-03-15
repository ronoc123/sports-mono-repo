using FluentValidation;

namespace Application.BacktestIntegrity.Commands.CreateBacktestSession;

public class CreateBacktestSessionCommandValidator : AbstractValidator<CreateBacktestSessionCommand>
{
    public CreateBacktestSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
    }
}
