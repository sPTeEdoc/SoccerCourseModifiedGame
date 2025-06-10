using Godot;
using System;

public partial class Node2d : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Hello from C# to Godot :)");
		//System.Console.WriteLine("Hello");
		Season.Instance.ResetSeason();
		Season.Instance.ScheduleSeason();
		GD.Print("Hello from C# to Godot :)");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		System.Console.WriteLine("Hello");
	}
}
