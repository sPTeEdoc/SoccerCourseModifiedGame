using Godot;
using System.Collections.Generic;

public static class FlagHelper
{
    private static Dictionary<string, Texture2D> flagTextures = new();

    public static Texture2D GetTexture(string teamFlagName)
    {
        if (!flagTextures.ContainsKey(teamFlagName))
        {
            string path = $"res://assets/art/ui/flags/flag-{teamFlagName.ToLower()}.png";
            var texture = GD.Load<Texture2D>(path);
            flagTextures[teamFlagName] = texture;
        }
        return flagTextures[teamFlagName];
    }
}
