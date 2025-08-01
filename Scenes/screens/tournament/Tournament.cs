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

    public string Winner { get; private set; } = "";

    public override void _Ready()
    {
        dataLoader = GetNode<DataLoader>("/root/DataLoader");
        var allCountries = dataLoader.GetCountries();
        var tournamentCountries = allCountries.GetRange(1, 8);
        Random random = new Random();
        for (int i = tournamentCountries.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (tournamentCountries[i], tournamentCountries[j]) = (tournamentCountries[j], tournamentCountries[i]);
        }
        CreateBracket(Stage.QuarterFinals, tournamentCountries);
    }

    private void CreateBracket(Stage stage, List<string> countries)
    {
        for (int i = 0; i < countries.Count / 2; i++)
        {
            var match = new Match(countries[i * 2], countries[i * 2 + 1]);
            Matches[stage].Add(match);
        }
    }

    public void Advance()
    {
        if (CurrentStage >= Stage.Complete)
            return;

        var stageMatches = Matches[CurrentStage];
        var stageWinners = new List<string>();

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