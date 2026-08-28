using Godot;
using System;
using System.Collections.Generic;

public partial class GameManagement : Node
{
    // Static reference so other scripts can still find it easily
    public static GameManagement Instance { get; private set; }
    
    // Drop the 'static' keyword from these so they belong to the instance
    public Dictionary<int, Team> TeamsDictionary { get; set; } = new Dictionary<int, Team>();
    public int PlayerID { get; set; } = 0;

    public override void _Ready()
    {
        Instance = this;
    }
}