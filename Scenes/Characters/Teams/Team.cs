using Godot;
using System;
using System.Collections.Generic;

public partial class Team
{
    public int TeamID;
    public string Name = "France";
    public string JerseyColorA = "#002395";
	public string Jersey_color_B = "#0023D5";
    public string Jersey_color_C = "";
	public string ShortsColor = "#002395";
	public string SocksColor = "#C41C1C";
    public List<PlayerResource> startingRoster = new List<PlayerResource>();
    public Team(int teamID, string name, string jerseyColorA, string jersey_color_B, string jersey_color_c,
        string shortsColor, string socksColor)
    {
        this.TeamID = teamID;
        this.Name = name;
        this.JerseyColorA = jerseyColorA;
        this.Jersey_color_B = jersey_color_B;
        this.Jersey_color_C = jersey_color_c;
        this.ShortsColor = shortsColor;
        this.SocksColor = socksColor;
    }
}
