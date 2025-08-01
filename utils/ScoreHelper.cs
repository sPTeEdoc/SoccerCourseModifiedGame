using Godot;

public static class ScoreHelper
{
    public static string GetScoreText(Match currentMatch)
    {
        return $"{currentMatch.GoalsHome} - {currentMatch.GoalsAway}";
    }

    public static string GetCurrentScoreInfo(Match currentMatch)
    {
        if (currentMatch.IsTied())
        {
            return $"TEAMS ARE TIED {currentMatch.GoalsHome} - {currentMatch.GoalsAway}";
        }
        else
        {
            return $"{currentMatch.Winner} LEADS {currentMatch.FinalScore}";
        }
    }

    public static string GetFinalScoreInfo(Match currentMatch)
    {
        return $"{currentMatch.Winner} WINS {currentMatch.FinalScore}";
    }
}
