using Godot;
using System;

public partial class SoccerGame : Node
{
    public enum ScreenType { MainMenu, TeamSelection, Tournament, InGame }

    private Screen currentScreen = null;
    private ScreenFactory screenFactory = new();

    public override void _Ready()
    {
        AddChild(screenFactory);
        SwitchScreen(ScreenType.MainMenu);
    }

    private void SwitchScreen(ScreenType screen, ScreenData data = null)
    {
        if (currentScreen != null)
            currentScreen.QueueFree();

        if (data == null)
            data = new ScreenData();

        currentScreen = screenFactory.GetFreshScreen(screen);
        currentScreen.Setup(this, data);
        currentScreen.ScreenTransitionRequested += SwitchScreen;

        CallDeferred("add_child", currentScreen);
    }
}