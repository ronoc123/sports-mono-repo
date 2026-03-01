namespace Domain.H2H.Services;

/// <summary>
/// Determines H2H match outcomes using a weighted probability formula based on
/// the delta between the fan's team overall and the bot's team overall.
/// </summary>
public static class BotResolutionEngine
{
    private const double WinCap = 0.85;
    private const double WinFloor = 0.15;
    private const double BaseWinRate = 0.5;
    private const double DeltaScaleDivisor = 200.0;

    /// <summary>
    /// Returns true if the fan wins.
    /// <list type="bullet">
    ///   <item>delta > 0: winProb = Min(0.5 + delta/200, 0.85)</item>
    ///   <item>delta &lt; 0: winProb = Max(0.5 + delta/200, 0.15)</item>
    ///   <item>delta = 0: winProb = 0.50</item>
    /// </list>
    /// </summary>
    public static bool FanWins(int fanOverall, int botOverall, Random? rng = null)
    {
        rng ??= Random.Shared;

        int delta = fanOverall - botOverall;

        double winProbability = BaseWinRate + (delta / DeltaScaleDivisor);
        winProbability = Math.Clamp(winProbability, WinFloor, WinCap);

        return rng.NextDouble() < winProbability;
    }

    /// <summary>Returns the win probability for a given delta (exposed for testing).</summary>
    public static double WinProbability(int fanOverall, int botOverall)
    {
        int delta = fanOverall - botOverall;
        return Math.Clamp(BaseWinRate + (delta / DeltaScaleDivisor), WinFloor, WinCap);
    }
}
