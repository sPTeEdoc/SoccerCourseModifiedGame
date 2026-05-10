using Godot;
using System;
using System.Collections.Generic;
using System.Linq; // Required for .FirstOrDefault()

public partial class DataLoader : Node
{
    private List<string> countries = new() { "DEFAULT" };
    private Dictionary<string, List<PlayerResource>> squads = new();
    private Dictionary<string, string> colorHexComboJerseyA = new Dictionary<string, string>();
    private Dictionary<string, string> colorHexComboJerseyB = new Dictionary<string, string>();
    private Dictionary<string, string> colorHexComboJerseyC = new Dictionary<string, string>();
    private Dictionary<string, string> socksCombo = new Dictionary<string, string>();
    private Dictionary<string, string> shortsCombo = new Dictionary<string, string>();
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
            string country = team["country"].AsString();
            countries.Add(country);
            string hexColor = team["jersey_color"].AsString();
            string hexColorB = team["jersey_color_B"].AsString();
            string hexColorC = "#000000";
            if (team.ContainsKey("jersey_color_C"))
                hexColorC = team["jersey_color_C"].AsString();
            string shortsColor = team["shorts"].AsString();
            string socksColor = team["socks"].AsString();
            colorHexComboJerseyA.Add(country, hexColor);
            colorHexComboJerseyB.Add(country, hexColorB);
            colorHexComboJerseyC.Add(country, hexColorC);
            shortsCombo.Add(country, shortsColor);
            socksCombo.Add(country, socksColor);

            if (!squads.ContainsKey(country))
                squads[country] = new List<PlayerResource>();

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
                squads[country].Add(resource);
            }

            if (players.Count != 6)
                GD.PrintErr($"Squad for {country} does not have exactly 6 players.");
        }

        file.Close();
    }

    public List<PlayerResource> GetSquad(string country)
    {
        return squads.TryGetValue(country, out var list) ? list : new List<PlayerResource>();
    }

    public List<string> GetCountries()
    {
        return countries;
    }

    public string GetJerseyColorA(string country)
    {
        return colorHexComboJerseyA[country];
    }

    public string GetJerseyColorB(string country)
    {
        return colorHexComboJerseyB[country];
    }

    public string GetJerseyColorC(string country)
    {
        if (colorHexComboJerseyC.ContainsKey(country))
            return colorHexComboJerseyC[country];
        return "#000000";
    }

    public string GetSocksColor(string country)
    {
        return socksCombo[country];
    }

    public string GetShortsColor(string country)
    {
        return shortsCombo[country];
    }

    public string GetSkinHex(string plyrName)
    {
        return skinColorCombo[plyrName];
    }
}