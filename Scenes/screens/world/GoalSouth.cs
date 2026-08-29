using Godot;
using System;

public partial class GoalSouth : Node2D
{
    [Export] private Area2D backNetAreaUpper;
    [Export] private Area2D backNetAreaLeft;
    [Export] private Area2D backNetAreaRight;
    [Export] private Area2D scoringArea;
    [Export] private TileMapLayer layer;
    [Export] private Area2D ballAtBackOfNet;
    [Export] private Area2D ballAwayFromBackOfNet;
    [Export] private Area2D crossBarPlus;
    [Export] public float CrossbarHeight { get; set; } = 32f;

    private GameEvents gameEvents;
    private SoundPlayer soundPlayer;

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");

        backNetAreaUpper.BodyEntered += OnBallEnterBackNet;
        backNetAreaUpper.BodyExited += OnBallExitBackNet; // Add exit monitoring

        backNetAreaRight.BodyEntered += OnBallEnterBackNet;
        backNetAreaRight.BodyExited += OnBallExitBackNet; // Add exit monitoring

        backNetAreaLeft.BodyEntered += OnBallEnterBackNet;
        backNetAreaLeft.BodyExited += OnBallExitBackNet; // Add exit monitoring
        scoringArea.BodyEntered += OnBallEnterScoringArea;

        ballAtBackOfNet.BodyEntered += OnBallAtBackOfNet;
        ballAwayFromBackOfNet.BodyEntered += OnBallAwayFromBackOfNet;

        crossBarPlus.BodyEntered += OnBallAtCrossBar;
    }

    private void OnBallAtCrossBar(Node body)
    {
        if (body is Ball ball)
        {
            ball.CollisionMask = 33;
            if (ball.Height <= ball.CrossbarHeight)
            {
                ball.CollisionMask = 33;
            }
            else
            {
                ball.CollisionMask = 161;
            }
        }
    }

    private void OnBallAwayFromBackOfNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                layer.ZIndex = 0;
            }
        }
    }

    private void OnBallAtBackOfNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                layer.ZIndex = 2;
            }
        }
    }

    private void OnBallEnterBackNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                ball.IsInNet = true;
            }
        }
    }

    private void OnBallExitBackNet(Node body)
    {
        if (body is Ball ball)
        {
            ball.IsInNet = false;
        }
    }

    bool goalCounted = false;

    private void OnBallEnterScoringArea(Node body)
    {
        if (body is Ball ball)
        {
            // 🎯 FIX: Only score if the ball is low enough to go under the crossbar
            if (ball.Height <= ball.CrossbarHeight)
            {
                if (!goalCounted)
                {
                    goalCounted = true;
                    soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
                    GD.Print("GOAL! The ball went under the bar.");
                }
                // gameEvents.EmitSignal("TeamScored", country);
            }
            else
            {
                GD.Print("MISS! The shot sailed over the crossbar.");
            }
        }
    }
}
