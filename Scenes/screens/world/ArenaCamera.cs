using Godot;
using System;

public partial class ArenaCamera : Camera2D
{
    private const float DistanceTarget = 100f;
    private const int DurationShake = 120;
    private const int ShakeIntensity = 5;
    private const int SmoothingBallCarried = 2;
    private const int SmoothingBallDefault = 8;
    private ulong timeStartShake = Time.GetTicksMsec();

    [Export] public Ball Ball { get; set; }

    public override void _Ready()
    {
        Ball = GetNode<Ball>("../ActorsContainer/Ball");
        var gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.EmitImpact(Ball.Position, false);
    }

    public override void _Process(double delta)
    {
        Vector2 targetPosition;

        // Vector2 velocityDir = Ball.Velocity.Normalized();
        targetPosition = Ball.Position;
        PositionSmoothingSpeed = SmoothingBallDefault;

        // Smoothly move toward biased target position
        Position = Position.Lerp(targetPosition, 0.1f);
    }
}