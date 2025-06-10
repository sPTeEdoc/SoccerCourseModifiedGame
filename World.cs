using Godot;
using System;

public partial class World : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		int x = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		int x = 0;
		System.Console.WriteLine("hello");
		GD.Print("hello");
	}
}
