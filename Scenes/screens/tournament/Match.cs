using Godot;
using System;

public partial class Match : Node
{
    public string CountryHome { get; private set; }
    public string CountryAway { get; private set; }

    public int GoalsHome { get; private set; } = 0;
    public int GoalsAway { get; private set; } = 0;

    public string FinalScore { get; private set; } = "";
    public string Winner { get; private set; } = "";

    public Match(string teamHome, string teamAway)
    {
        CountryHome = teamHome;
        CountryAway = teamAway;
    }

    public bool IsTied()
    {
        return GoalsHome == GoalsAway;
    }

    public bool HasSomeoneScored()
    {
        return GoalsHome > 0 || GoalsAway > 0;
    }

    public void IncreaseScore(string countryScoredOn)
    {
        if (countryScoredOn == CountryHome)
            GoalsAway += 1;
        else
            GoalsHome += 1;

        UpdateMatchInfo();
    }

    private void UpdateMatchInfo()
    {
        Winner = (GoalsHome > GoalsAway) ? CountryHome : CountryAway;
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