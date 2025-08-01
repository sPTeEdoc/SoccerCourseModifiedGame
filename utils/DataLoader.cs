using Godot;
using System;
using System.Collections.Generic;

public partial class DataLoader : Node
{
    private List<string> countries = new() { "DEFAULT" };
    private Dictionary<string, List<PlayerResource>> squads = new();

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

            if (!squads.ContainsKey(country))
                squads[country] = new List<PlayerResource>();

            var players = team["players"].AsGodotArray();
            foreach (Godot.Variant playerVariant in players)
            {
                var player = playerVariant.AsGodotDictionary();
                string fullname = player["name"].AsString();
                var skin = (PlayerCharacter.SkinColor)(int)player["skin"];
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
}