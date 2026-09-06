using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class Match : Node
{
    public int HomeTeam { get; private set; }
    public int AwayTeam { get; private set; }
    public int TeamKickingOff { get; set; } = 2;
    public int LastBallCarrier { get; set; } = 1;
    public int TeamWithPossession
    {
        get
        {
            return GameManagement.Instance.PlayerDictionary[LastBallCarrier].TeamID;
        }
    }
    public float HomeTeamPossessionTime { get; set; } = 0;
    public float AwayTeamPossessionTime { get; set; } = 0;

    public int GoalsHome { get; private set; } = 0;
    public int GoalsAway { get; private set; } = 0;

    public string FinalScore { get; private set; } = "";
    public int Winner { get; private set; } = -1;
    public int Half { get; set; } = 1;

    public List<GoalSummary> awayPlayerGoalTrackerTraditional = new List<GoalSummary>();
    public List<GoalSummary> homePlayerGoalTrackerTraditional = new List<GoalSummary>();


    public Match(int teamHome, int teamAway)
    {
        HomeTeam = teamHome;
        AwayTeam = teamAway;
        TeamKickingOff = teamHome;
    }

    public void AdvanceHalf()
    {
        Half++;
        // Alternate kickoff team each half
        TeamKickingOff = (TeamKickingOff == HomeTeam) ? AwayTeam : HomeTeam;
    }

    public bool IsTied()
    {
        return GoalsHome == GoalsAway;
    }

    public bool HasSomeoneScored()
    {
        return GoalsHome > 0 || GoalsAway > 0;
    }

    public void IncreaseScore(int teamScoredOn, float TimeElapsed)
    {
        UpdateScore(teamScoredOn, TimeElapsed);

        UpdateMatchInfo();
    }

    private void UpdateScore(int teamScoredOn, float TimeElapsed)
    {
        List<GoalSummary> goalTrackerTraditional = new List<GoalSummary>();
        bool homeTeamScored = false;

        if (teamScoredOn == HomeTeam)
        {
            GoalsAway += 1;
            goalTrackerTraditional = awayPlayerGoalTrackerTraditional;
        }
        else
        {
            homeTeamScored = true;
            GoalsHome += 1;
            goalTrackerTraditional = homePlayerGoalTrackerTraditional;
        }

        PlayerResource shooter = GameManagement.Instance.PlayerDictionary[LastBallCarrier];

        GoalSummary goalSummary = goalTrackerTraditional.Find(x => x.PlayerID == shooter.PlayerID);
        if (goalSummary is null)
        {
            goalSummary = new GoalSummary();
            goalTrackerTraditional.Add(goalSummary);
        }

        goalSummary.MinutesScored.Add(TimeElapsed);
        if (homeTeamScored)
            goalSummary.TeamWhoScoredGoal = HomeTeam;
        else
            goalSummary.TeamWhoScoredGoal = AwayTeam;
        if (shooter.TeamID != goalSummary.TeamWhoScoredGoal)
            goalSummary.IsOwnGoal = true;
        goalSummary.PlayerID = shooter.PlayerID;
    }

    private void UpdateMatchInfo()
    {
        Winner = (GoalsHome > GoalsAway) ? HomeTeam : (GoalsAway > GoalsHome ? AwayTeam : -1);
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