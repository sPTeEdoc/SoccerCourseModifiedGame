using Godot;
using System;
using System.Collections.Generic;

public partial class Tournament : Node
{
    public enum Stage { QuarterFinals, SemiFinals, Final, Complete }
    public DataLoader dataLoader;

    public Stage CurrentStage { get; private set; } = Stage.QuarterFinals;
    public Dictionary<Stage, List<Match>> Matches { get; private set; } = new()
    {
        { Stage.QuarterFinals, new List<Match>() },
        { Stage.SemiFinals, new List<Match>() },
        { Stage.Final, new List<Match>() }
    };

    public int Winner { get; private set; } = -1;

    public override void _Ready()
    {
        dataLoader = GetNode<DataLoader>("/root/DataLoader");
        var allTeams = dataLoader.GetTeams();
        var tournamenTeams = allTeams.GetRange(0, 8);
        Random random = new Random();
        for (int i = tournamenTeams.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (tournamenTeams[i], tournamenTeams[j]) = (tournamenTeams[j], tournamenTeams[i]);
        }
        CreateBracket(Stage.QuarterFinals, tournamenTeams);
    }

    private void CreateBracket(Stage stage, List<int> teams)
    {
        for (int i = 0; i < teams.Count / 2; i++)
        {
            var match = new Match(teams[i * 2], teams[i * 2 + 1]);
            Matches[stage].Add(match);
        }
    }

    public void Advance()
    {
        if (CurrentStage >= Stage.Complete)
            return;

        var stageMatches = Matches[CurrentStage];
        var stageWinners = new List<int>();

        foreach (var match in stageMatches)
        {
            match.Resolve();
            stageWinners.Add(match.Winner);
        }

        CurrentStage++;
        if (CurrentStage == Stage.Complete)
        {
            Winner = stageWinners[0];
        }
        else
        {
            CreateBracket(CurrentStage, stageWinners);
        }
    }
}