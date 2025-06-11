using Godot;
using System;

public partial class Menu : Control
{
    [Export] private string _targetScene = "calendar.tscn";

    public override void _Ready()
    {

    }

    public void OnButtonPressed()
    {
        // Node2D container = (Node2D)GetNode("/root/SceneTransition");
        GetNode<SceneTransition>("/root/SceneTransition").ChangeToScene(_targetScene);
    }
}
