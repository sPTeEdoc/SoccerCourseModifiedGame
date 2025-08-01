using FunnyOldGame;
using FunnyOldGameRedux;
using Godot;
using System;

public partial class Menu : Control
{
    [Export] private string _targetScene = "calendar.tscn";
    [Export] private string _fieldScene = "/Scenes/Field.tscn";

    public override void _Ready()
    {
        int x = 0;
    }

    public void OnButtonPressed()
    {
        // Node2D container = (Node2D)GetNode("/root/SceneTransition");
        GetNode<SceneTransition>("/root/SceneTransition").ChangeToScene(_targetScene);
    }

    public void OnExhibitionButtonPressed()
    {
        TeamRepository.Instance.LoadTeams();
        Season.Instance.ScheduleSeason();
        Team homeTeam = TeamRepository.Instance.teamNameDict["Manchester United"];
        Team awayTeam = TeamRepository.Instance.teamNameDict["Manchester City"];
        Game game = new Game(homeTeam, awayTeam, 45, false, true, false, 5, DateTime.Now, "Exhibition",
               1, "Exhibition", false);
        // Node2D container = (Node2D)GetNode("/root/SceneTransition");
        GetNode<SceneTransition>("/root/SceneTransition").ChangeToScene(_fieldScene);
    }
}
