namespace Domain.BacktestIntegrity;

/// <summary>
/// Value object holding submitted backtest statistics.
/// Expectancy = (WinRate × AvgRR) - (1 - WinRate)
/// </summary>
public record BacktestMetrics
{
    public int TotalTrades { get; init; }
    public decimal WinRate { get; init; }       // 0.0 – 1.0
    public decimal AvgRR { get; init; }          // Average risk:reward ratio
    public decimal Expectancy { get; init; }

    public static BacktestMetrics Compute(int totalTrades, decimal winRate, decimal avgRR)
    {
        var expectancy = (winRate * avgRR) - (1m - winRate);
        return new BacktestMetrics
        {
            TotalTrades = totalTrades,
            WinRate = winRate,
            AvgRR = avgRR,
            Expectancy = Math.Round(expectancy, 4)
        };
    }
}
