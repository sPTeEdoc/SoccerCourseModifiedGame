using Godot;
using System;
using System.Collections.Generic;

public class GoalSummary
{
    public int PlayerID { get; set; } = -1;
    public int TeamWhoScoredGoal { get; set; } = -1;
    public List<float> MinutesScored { get; set; } = new List<float>();
    public bool IsOwnGoal {get; set; } = false;
}
