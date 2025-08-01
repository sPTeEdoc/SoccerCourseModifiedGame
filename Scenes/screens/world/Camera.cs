using Godot;
using System;

public partial class Camera : Camera2D
{
    private const float DistanceTarget = 100f;
    private const int DurationShake = 120;
    private const int ShakeIntensity = 5;
    private const int SmoothingBallCarried = 2;
    private const int SmoothingBallDefault = 8;

    private bool isShaking = false;
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
        if (Ball.Carrier != null)
        {
            Position = Ball.Carrier.Position + Ball.Carrier.heading * DistanceTarget;
            PositionSmoothingSpeed = SmoothingBallCarried;
        }
        else
        {
            Position = Ball.Position;
            PositionSmoothingSpeed = SmoothingBallDefault;
        }

        if (isShaking && Time.GetTicksMsec() - timeStartShake < DurationShake)
        {
            Offset = new Vector2(
                GD.RandRange(-ShakeIntensity, ShakeIntensity),
                GD.RandRange(-ShakeIntensity, ShakeIntensity)
            );
        }
        else
        {
            isShaking = false;
            Offset = Vector2.Zero;
        }
    }

    private void OnImpactReceived(Vector2 impactPosition, bool isHighImpact)
    {
        if (isHighImpact)
        {
            isShaking = true;
            timeStartShake = Time.GetTicksMsec();
        }
    }
}