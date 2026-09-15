using Godot;
using System;
using System.Collections.Generic;

public partial class GameManagement : Node
{
    // Static reference so other scripts can still find it easily
    public static GameManagement Instance { get; private set; }

    // Drop the 'static' keyword from these so they belong to the instance
    public Dictionary<int, Team> TeamsDictionary { get; set; } = new Dictionary<int, Team>();
    public Dictionary<int, PlayerResource> PlayerDictionary { get; set; } = new Dictionary<int, PlayerResource>();
    public int PlayerID { get; set; } = 0;
    public bool IsOnPracticeField { get; set; } = false;
    public Random rand = new Random();

    public override void _Ready()
    {
        Instance = this;
    }

    public void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}