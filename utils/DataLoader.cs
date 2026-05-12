using Godot;
using System;
using System.Collections.Generic;
using System.Linq; // Required for .FirstOrDefault()

public partial class DataLoader : Node
{
    private Dictionary<string, string> skinColorCombo = new Dictionary<string, string>();

    public override void _Ready()
    {
        var file = FileAccess.Open("res://assets/json/squads.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError("Could not find or load squads.json");
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
            string hexColor = team["jersey_color"].AsString();
            string hexColorB = team["jersey_color_B"].AsString();
            string hexColorC = "#000000";
            if (team.ContainsKey("jersey_color_C"))
                hexColorC = team["jersey_color_C"].AsString();
            string shortsColor = team["shorts"].AsString();
            string socksColor = team["socks"].AsString();
            Team club = new Team(tID, teamName, hexColor, hexColorB, hexColorC,
                shortsColor, socksColor);

            var players = team["players"].AsGodotArray();
            foreach (Godot.Variant playerVariant in players)
            {
                var player = playerVariant.AsGodotDictionary();
                string fullname = player["name"].AsString();
                string skin = player["skin"].AsString();
                skinColorCombo.Add(fullname, skin);
                var role = (PlayerCharacter.Role)(int)player["role"];
                float speed = (float)player["speed"];
                float power = (float)player["power"];

                var resource = new PlayerResource(fullname, skin, role, speed, power);
                club.startingRoster.Add(resource);
            }

            GameManagement.teamsDictionary.Add(club.TeamID, club);

            if (players.Count != 6)
                GD.PrintErr($"Squad for {teamName} does not have exactly 6 players.");
        }

        file.Close();
    }

    public List<PlayerResource> GetSquad(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].startingRoster;
    }

    public List<int> GetTeams()
    {
        List<int> teams = new List<int>();
        foreach(KeyValuePair<int, Team> t in GameManagement.teamsDictionary)
        {
            teams.Add(t.Value.TeamID);
        }
        return teams;
    }

    public string GetJerseyColorA(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].JerseyColorA;
    }

    public string GetJerseyColorB(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].Jersey_color_B;
    }

    public string GetJerseyColorC(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].Jersey_color_C;
    }

    public string GetSocksColor(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].SocksColor;
    }

    public string GetShortsColor(int teamID)
    {
        return GameManagement.teamsDictionary[teamID].ShortsColor;
    }

    public string GetSkinHex(string plyrName)
    {
        return skinColorCombo[plyrName];
    }
}