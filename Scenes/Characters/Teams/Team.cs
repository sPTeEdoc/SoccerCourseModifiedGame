using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Team
{
    public int TeamID;
    public string Name = "Team Nibley";
    public string jersey_color = "#DA020E";
    public string shorts = "#0A0A0A";
    public string socks = "#FFFFFF";
    public string keeper_jersey = "#9AFF34";
    public string keeper_shorts = "#0A0A0A";
    public string keeper_socks = "#9AFF34";
    public List<PlayerResource> completeRoster = new List<PlayerResource>();
    public PlayerResource[] startingEleven = new PlayerResource[11];
    public PlayerResource[] startingSeven = new PlayerResource[7];
    public Vector2 Direction = Vector2.Up;
    public bool TeamIsKickingOff = false;
    public Enums.Formations ElevenASide = Enums.Formations.FourFourTwo;
    public Enums.Formations SevenASide = Enums.Formations.TwoTwoTwo;
    public Enums.Formations Formation = Enums.Formations.FourFourTwo;
    [Export] public ArenaGoal targetGoal; // The goal this team is trying to score in
    public Team(int teamID, string name, string jersey_color, string shorts, string socks,
        string keeper_jersey, string keeper_shorts, string keeper_socks)
    {
        this.TeamID = teamID;
        this.Name = name;
        this.jersey_color = jersey_color;
        this.shorts = shorts;
        this.socks = socks;
        this.keeper_jersey = keeper_jersey;
        this.keeper_shorts = keeper_shorts;
        this.keeper_socks = keeper_socks;
    }

    public void RedefinePositions(PlayerResource[] lineup)
    {
        int numberAlreadyAssigned = 1;
        lineup[0].Position = PlayerCharacter.OnFieldPositions.GOALIE;
        int numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfDefenders() + numberAlreadyAssigned; i >= numberAssignedPrev; i--)
        {
            lineup[i - 1].Position = PlayerCharacter.OnFieldPositions.DEFENSE;
            numberAlreadyAssigned++;
        }
        numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfMidfielders() + numberAlreadyAssigned; i >= numberAssignedPrev; i--)
        {
            lineup[i].Position = PlayerCharacter.OnFieldPositions.MIDFIELD;
            numberAlreadyAssigned++;
        }
        numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfForwards() + numberAlreadyAssigned; i >= numberAssignedPrev; i--)
        {
            lineup[i].Position = PlayerCharacter.OnFieldPositions.FORWARD;
        }
    }

    public void ConfigureLineup(ref PlayerResource[] lineup)
    {
        for (int i = 0; i < lineup.Length; i++)
        {
            if (lineup[i] is not null) lineup[i].Position = PlayerCharacter.OnFieldPositions.ANY;
        }

        lineup[0] = this.completeRoster
            .FindAll(x => x.Position == PlayerCharacter.OnFieldPositions.GOALIE
            || x.Position == PlayerCharacter.OnFieldPositions.ANY)
            .OrderByDescending(x => PlayerOverallCalculator.CalculateOverall(PlayerCharacter.OnFieldPositions.GOALIE, x)).FirstOrDefault();
        var selectedPlayers = new HashSet<PlayerResource>();
        selectedPlayers.Add(lineup[0]);
        lineup[0].Position = PlayerCharacter.OnFieldPositions.GOALIE;

        int numberAlreadyAssigned = 1;
        int numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfDefenders() + numberAlreadyAssigned; i > numberAssignedPrev; i--)
        {
            lineup[i - 1] = this.completeRoster
                .Where(x => (x.IsCaptain && !selectedPlayers.Contains(x)) || (!selectedPlayers.Contains(x) && (x.Position == PlayerCharacter.OnFieldPositions.DEFENSE || x.Position == PlayerCharacter.OnFieldPositions.ANY)))
                .OrderByDescending(x => PlayerOverallCalculator.CalculateOverall(PlayerCharacter.OnFieldPositions.DEFENSE, x))
                .FirstOrDefault();
            if (lineup[i - 1] != null) selectedPlayers.Add(lineup[i - 1]);
            lineup[i - 1].Position = PlayerCharacter.OnFieldPositions.DEFENSE;
            numberAlreadyAssigned++;
        }
        numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfMidfielders() + numberAlreadyAssigned - 1; i >= numberAssignedPrev; i--)
        {
            lineup[i] = this.completeRoster
                .Where(x => (x.IsCaptain && !selectedPlayers.Contains(x)) || (!selectedPlayers.Contains(x) && (x.Position == PlayerCharacter.OnFieldPositions.MIDFIELD || x.Position == PlayerCharacter.OnFieldPositions.ANY)))
                .OrderByDescending(x => PlayerOverallCalculator.CalculateOverall(PlayerCharacter.OnFieldPositions.MIDFIELD, x))
                .FirstOrDefault();
            if (lineup[i] != null) selectedPlayers.Add(lineup[i]);
            lineup[i].Position = PlayerCharacter.OnFieldPositions.MIDFIELD;
            numberAlreadyAssigned++;
        }
        numberAssignedPrev = numberAlreadyAssigned;
        for (int i = NumberOfForwards() + numberAlreadyAssigned - 1; i >= numberAssignedPrev; i--)
        {
            lineup[i] = this.completeRoster
                .Where(x => (x.IsCaptain && !selectedPlayers.Contains(x)) || (!selectedPlayers.Contains(x) && (x.Position == PlayerCharacter.OnFieldPositions.FORWARD || x.Position == PlayerCharacter.OnFieldPositions.ANY)))
                .OrderByDescending(x => PlayerOverallCalculator.CalculateOverall(PlayerCharacter.OnFieldPositions.FORWARD, x))
                .FirstOrDefault();
            if (lineup[i] != null) selectedPlayers.Add(lineup[i]);
            lineup[i].Position = PlayerCharacter.OnFieldPositions.FORWARD;
        }
    }

    public int NumberOfDefenders()
    {
        switch (Formation)
        {
            case Enums.Formations.OneThreeTwo:
            case Enums.Formations.OneTwoThree:
                return 1;
            case Enums.Formations.TwoOneThree:
            case Enums.Formations.TwoTwoTwo:
            case Enums.Formations.TwoThreeOne:
                return 2;
            case Enums.Formations.ThreeOneTwo:
            case Enums.Formations.ThreeTwoOne:
                return 3;
            case Enums.Formations.FourFourTwo:
            case Enums.Formations.FourThreeThree:
                return 4;
            default:
                return 2;
        }
    }

    public int NumberOfMidfielders()
    {
        switch (Formation)
        {
            case Enums.Formations.TwoOneThree:
            case Enums.Formations.ThreeOneTwo:
                return 1;
            case Enums.Formations.OneTwoThree:
            case Enums.Formations.ThreeTwoOne:
            case Enums.Formations.TwoTwoTwo:
                return 2;
            case Enums.Formations.TwoThreeOne:
            case Enums.Formations.FourThreeThree:
            case Enums.Formations.OneThreeTwo:
                return 3;
            case Enums.Formations.FourFourTwo:
                return 4;
            default:
                return 2;
        }
    }

    public int NumberOfForwards()
    {
        switch (Formation)
        {
            case Enums.Formations.ThreeTwoOne:
            case Enums.Formations.TwoThreeOne:
                return 1;
            case Enums.Formations.FourFourTwo:
            case Enums.Formations.OneThreeTwo:
            case Enums.Formations.TwoTwoTwo:
            case Enums.Formations.ThreeOneTwo:
                return 2;
            case Enums.Formations.FourThreeThree:
            case Enums.Formations.TwoOneThree:
            case Enums.Formations.OneTwoThree:
                return 3;
            default:
                return 2;
        }
    }
}