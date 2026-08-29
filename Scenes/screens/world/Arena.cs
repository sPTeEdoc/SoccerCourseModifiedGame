using Godot;
using System;

public partial class Arena : Screen
{
    private Timer gameOverTimer;
    public GameManager gameManager;

    public override void _Ready()
    {
        gameOverTimer = GetNode<Timer>("GameOverTimer");

        gameOverTimer.Timeout += OnTransition;

        var gameEvents = GetNode("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        gameEvents.Connect("GameOver", new Callable(this, nameof(OnGameOver)));

        gameManager.StartGame();
    }

    private void OnGameOver(string winner)
    {
        gameOverTimer.Start();
    }

    private void OnTransition()
    {
        
    }
}