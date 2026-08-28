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
            return $"{GameManagement.Instance.TeamsDictionary[currentMatch.Winner].Name} LEADS {currentMatch.FinalScore}";
        }
    }

    public static string GetFinalScoreInfo(Match currentMatch)
    {
        return $"{GameManagement.Instance.TeamsDictionary[currentMatch.Winner].Name} WINS {currentMatch.FinalScore}";
    }
}
