using Godot;
using System;

[GlobalClass]
public partial class PlayerStateShooting : PlayerState
{
    private const float SHOT_FREEZE_DURATION = 0.15f; // â Brief animation pause
    private float freezeTimer = 0f;

    public SoundPlayer soundPlayer;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");

        freezeTimer = SHOT_FREEZE_DURATION;
        player.Velocity = Vector2.Zero;
        player.InputLocked = true;

        // â Play kick animation
        PlayKickAnimation();

        // â Shoot immediately (no charge-up)
        ShootBall();
    }

    private void PlayKickAnimation()
    {
        float snappedAngle = Mathf.Round(player.heading.Angle() * 180f / MathF.PI / 45f) * 45f;
        int angleCheck = (int)snappedAngle;
        if (angleCheck == -180) angleCheck = 180;

        string animPrefix = "kick_";
        string directionStr = angleCheck switch
        {
            0 => "east",
            -45 => "northeast",
            -90 => "north",
            -135 => "northwest",
            180 => "west",
            135 => "southwest",
            90 => "south",
            45 => "southeast",
            _ => "south"
        };

        player.animatedSprite2D.Play(animPrefix + directionStr);
    }

    public override void _Process(double delta)
    {
        freezeTimer -= (float)delta;
        player.Velocity = Vector2.Zero;

        if (freezeTimer <= 0f)
        {
            player.InputLocked = false;
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }

    private void ShootBall()
    {
        // AI shots (if provided)
        if (stateData.ShotDirection != Vector2.Zero)
        {
            ball.Shoot(stateData.ShotDirection * stateData.ShotPower);
            return;
        }

        // Human player shots
        player.FaceTowardsTargetGoal();

        Vector2 goalCenter = player.targetGoal.GetCenterTargetPosition();
        float distanceToGoal = player.Position.DistanceTo(goalCenter);

        // â MUCH STRONGER BASE POWER
        float basePower = Mathf.Clamp(distanceToGoal / 2.2f, 140f, 320f); // Was 60-100

        // â Shooting skill multiplier (0.85x to 1.3x)
        float skillMultiplier = 0.85f + (player.GameAttributes.Shooting / 100f) * 0.45f;
        float finalPower = basePower * skillMultiplier;

        // Aim toward goal with slight randomization
        Vector2 targetSpot = player.targetGoal.GetRandomTargetPosition();
        Vector2 shotDirection = player.Position.DirectionTo(targetSpot);

        ball.Shoot(shotDirection * finalPower);
    }

    public override void OnAnimationComplete()
    {
        // Animation complete handled by timer instead
    }
}