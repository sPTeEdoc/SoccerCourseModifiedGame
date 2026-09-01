using Godot;
using System;

public partial class Match : Node
{
    public int HomeTeam { get; private set; }
    public int AwayTeam { get; private set; }
    public int TeamKickingOff {get;set;} = 2;

    public int GoalsHome { get; private set; } = 0;
    public int GoalsAway { get; private set; } = 0;

    public string FinalScore { get; private set; } = "";
    public int Winner { get; private set; } = -1;

    public Match(int teamHome, int teamAway)
    {
        HomeTeam = teamHome;
        AwayTeam = teamAway;
        TeamKickingOff = teamHome;
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
        if (teamScoredOn == HomeTeam)
            GoalsAway += 1;
        else
            GoalsHome += 1;

        UpdateMatchInfo();
    }

    private void UpdateMatchInfo()
    {
        Winner = (GoalsHome > GoalsAway) ? HomeTeam : AwayTeam;
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