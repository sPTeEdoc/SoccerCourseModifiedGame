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
        Vector2 targetPosition;

        if (Ball.Carrier != null)
        {
            // Bias toward heading direction of the carrier
            Vector2 heading = Ball.Carrier.heading.Normalized();
            targetPosition = Ball.Carrier.Position + heading * DistanceTarget;
            PositionSmoothingSpeed = SmoothingBallCarried;
        }
        else
        {
            // Bias toward velocity direction of the ball
            Vector2 velocityDir = Ball.Velocity.Normalized();
            targetPosition = Ball.Position + velocityDir * DistanceTarget;
            PositionSmoothingSpeed = SmoothingBallDefault;
        }

        // Smoothly move toward biased target position
        Position = Position.Lerp(targetPosition, 0.1f);

        // Shake logic (unchanged)
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