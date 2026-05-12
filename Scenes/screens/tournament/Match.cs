using Godot;
using System;

public partial class Match : Node
{
    public int TeamHome { get; private set; }
    public int TeamAway { get; private set; }

    public int GoalsHome { get; private set; } = 0;
    public int GoalsAway { get; private set; } = 0;

    public string FinalScore { get; private set; } = "";
    public int Winner { get; private set; } = -1;

    public Match(int teamHome, int teamAway)
    {
        TeamHome = teamHome;
        TeamAway = teamAway;
    }

    public bool IsTied()
    {
        return GoalsHome == GoalsAway;
    }

    public bool HasSomeoneScored()
    {
        return GoalsHome > 0 || GoalsAway > 0;
    }

    public void IncreaseScore(int teamScoredOn)
    {
        if (teamScoredOn == TeamHome)
            GoalsAway += 1;
        else
            GoalsHome += 1;

        UpdateMatchInfo();
    }

    private void UpdateMatchInfo()
    {
        Winner = (GoalsHome > GoalsAway) ? TeamHome : TeamAway;
        FinalScore = $"{Math.Max(GoalsHome, GoalsAway)} - {Math.Min(GoalsHome, GoalsAway)}";
    }

    public void Resolve()
    {
        while (IsTied())
        {
            GoalsHome = (int)GD.Randi() % 6;
            GoalsAway = (int)GD.Randi() % 6;
        }

        UpdateMatchInfo();
    }
}