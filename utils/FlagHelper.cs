using Godot;
using System.Collections.Generic;

public static class FlagHelper
{
    private static Dictionary<string, Texture2D> flagTextures = new();

    public static Texture2D GetTexture(string country)
    {
        if (!flagTextures.ContainsKey(country))
        {
            string path = $"res://assets/art/ui/flags/flag-{country.ToLower()}.png";
            var texture = GD.Load<Texture2D>(path);
            flagTextures[country] = texture;
        }
        return flagTextures[country];
    }
}
