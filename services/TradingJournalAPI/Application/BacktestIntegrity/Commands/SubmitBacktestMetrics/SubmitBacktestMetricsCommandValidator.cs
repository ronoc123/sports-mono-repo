using FluentValidation;

namespace Application.BacktestIntegrity.Commands.SubmitBacktestMetrics;

public class SubmitBacktestMetricsCommandValidator : AbstractValidator<SubmitBacktestMetricsCommand>
{
    public SubmitBacktestMetricsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TotalTrades).GreaterThan(0);
        RuleFor(x => x.WinRate).InclusiveBetween(0m, 1m).WithMessage("WinRate must be between 0 and 1.");
        RuleFor(x => x.AvgRR).GreaterThan(0m).WithMessage("AvgRR must be greater than 0.");
    }
}
