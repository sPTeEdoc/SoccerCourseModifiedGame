using Godot;
using System;
using System.Collections.Generic;

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
    public List<PlayerResource> startingRoster = new List<PlayerResource>();
    public Vector2 Direction = Vector2.Up;
    public bool TeamIsKickingOff = false;
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
}