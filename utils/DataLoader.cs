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

            var players = team["players"].AsGodotArray();
            foreach (Godot.Variant playerVariant in players)
            {
                var player = playerVariant.AsGodotDictionary();
                int playerID = (int)player["id"];
                int number = (int)player["number"];
                string fullname = player["name"].AsString();
                string skin = player["skin"].AsString();
                string hair = player["hair"].AsString();
                skinColorCombo.Add(playerID, skin);
                var role = (PlayerCharacter.Role)(int)player["role"];
                float speed = (float)player["speed"];
                float power = (float)player["power"];
                float pass = (float)player["pass"];
                float control = (float)player["control"];

                var resource = new PlayerResource(playerID, fullname, skin, hair, role, number, pass, control, speed, power);
                club.startingRoster.Add(resource);
            }

            GameManagement.Instance.TeamsDictionary.Add(club.TeamID, club);
        }

        file.Close();
    }

    public List<PlayerResource> GetSquad(int teamID)
    {
        return GameManagement.Instance.TeamsDictionary[teamID].startingRoster;
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