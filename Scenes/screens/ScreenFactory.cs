using Godot;
using System;
using System.Collections.Generic;

public partial class ScreenFactory : Node
{
    private Dictionary<SoccerGame.ScreenType, PackedScene> screens;

    public override void _Ready()
    {
        screens = new Dictionary<SoccerGame.ScreenType, PackedScene>
        {
            { SoccerGame.ScreenType.InGame, GD.Load<PackedScene>("res://scenes/screens/world/world_screen.tscn") },
            { SoccerGame.ScreenType.MainMenu, GD.Load<PackedScene>("res://scenes/screens/main_menu/main_menu_screen.tscn") },
            { SoccerGame.ScreenType.TeamSelection, GD.Load<PackedScene>("res://scenes/screens/team_selection/team_selection_screen.tscn") },
            { SoccerGame.ScreenType.Tournament, GD.Load<PackedScene>("res://scenes/screens/tournament/tournament_screen.tscn") }
        };
    }

    public Screen GetFreshScreen(SoccerGame.ScreenType type)
    {
        if (screens != null)
        {
            int x = 0;
        }
        if (!screens.ContainsKey(type))
            {
                GD.PushError($"Screen '{type}' does not exist");
                return null;
            }

        return screens[type].Instantiate<Screen>();
    }
}