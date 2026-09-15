using Godot;
using System;
using System.Collections.Generic;
using System.Linq; // Required for .FirstOrDefault()

public partial class DataLoader : Node
{
    private Dictionary<int, string> skinColorCombo = new Dictionary<int, string>();

    public override void _Ready()
    {
        LoadSquads("squads2.json");
    }

    private void LoadSquads(string jsonFilePath)
    {
        var file = FileAccess.Open($"res://assets/json/{jsonFilePath}", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError("Could not find or load squads2.json");
            return;
        }

        var jsonText = file.GetAsText();
        var json = new Json();
        var parseResult = json.Parse(jsonText);

        if (parseResult != Error.Ok)
        {
            GD.PushError("Could not parse squads.json");
            return;
        }

        var data = json.Data.AsGodotArray();
        foreach (Godot.Variant teamVariant in data)
        {
            var team = teamVariant.AsGodotDictionary();
            var teamID = team["teamID"].AsString();

            int tID = Int32.Parse(teamID);
            string teamName = team["teamName"].AsString();
            string jersey_color = team["jersey_color"].AsString();
            string shorts = team["shorts"].AsString();
            string socks = team["socks"].AsString();

            string keeper_jersey = team["keeper_jersey"].AsString();
            string keeper_shorts = team["keeper_shorts"].AsString();
            string keeper_socks = team["keeper_socks"].AsString(); ;

            Team club = new Team(tID, teamName, jersey_color, shorts, socks,
                keeper_jersey, keeper_shorts, keeper_socks);

            string ElevenASide = team["elevenASideFormation"].AsString();
            string sevenASideFormation = team["sevenASideFormation"].AsString();
            club.SevenASide = (Enums.Formations)Int32.Parse(sevenASideFormation);
            club.ElevenASide = (Enums.Formations)Int32.Parse(ElevenASide);

            var players = team["players"].AsGodotArray();
            foreach (Godot.Variant playerVariant in players)
            {
                var player = playerVariant.AsGodotDictionary();
                int playerID = (int)player["id"];
                int number = (int)player["number"];
                string firstName = player["firstName"].AsString();
                string lastName = player["lastName"].AsString();
                string altName = player["altName"].AsString();
                string skin = player["skin"].AsString();
                string hair = player["hair"].AsString();
                skinColorCombo.Add(playerID, skin);
                var role = (PlayerCharacter.OnFieldPositions)(int)player["role"];
                int offense = (int)player["offense"];
                int defense = (int)player["defense"];
                int awareness = (int)player["awareness"];
                int shooting = (int)player["shooting"];
                int passing = (int)player["passing"];
                int speed = (int)player["speed"];
                int dribble = (int)player["dribble"];
                int strength = (int)player["strength"];
                int toughness = (int)player["toughness"];
                int athleticism = (int)player["athleticism"];
                int popularity = (int)player["popularity"];
                int header = (int)player["header"];
                int save = (int)player["save"];
                int reflexes = (int)player["reflexes"];
                int special = (int)player["special"];
                bool isCaptain = ParseBool(player, "isCaptain");

                var resource = new PlayerResource(playerID, firstName, lastName, altName, skin, hair, role, number, tID, offense,
                    defense, awareness, shooting, passing, speed, dribble, strength, toughness, athleticism, popularity,
                    header, save, reflexes, special, isCaptain);
                club.completeRoster.Add(resource);
                GameManagement.Instance.PlayerDictionary.Add(playerID, resource);
            }

            GameManagement.Instance.TeamsDictionary.Add(club.TeamID, club);
        }

        file.Close();
    }

    private bool ParseBool(Godot.Collections.Dictionary var, string s)
    {
        bool boolVal = false;
        if (var.ContainsKey(s))
        {
            string val = var[s].AsString();
            if (val == "1")
                boolVal = true;
            else if (val == "true")
                boolVal = true;
        }
        return boolVal;
    }

    public PlayerResource[] GetStartingEleven(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].startingEleven;
    }

    public PlayerResource[] GetStartingSeven(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].startingSeven;
    }

    public List<int> GetTeams()
    {
        List<int> teams = new List<int>();
        foreach (KeyValuePair<int, Team> t in GameManagement.Instance.TeamsDictionary)
        {
            teams.Add(t.Value.TeamID);
        }
        return teams;
    }

    public string GetJerseyColor(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].jersey_color;
    }

    public string GetShortsColor(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].shorts;
    }

    public string GetSocks(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].socks;
    }

    public string GetKeeperJerseyColor(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].keeper_jersey;
    }

    public string GetKeeperShorts(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].keeper_shorts;
    }

    public string GetKeeperSocks(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].keeper_socks;
    }

    public string GetSkinHex(int plyrID)
    {
        return skinColorCombo[plyrID];
    }
}