using Godot;
using System;

public partial class BallStateShot : BallState
{
    private const int DurationShot = 800; // â Reduced from 1000ms
    private const float ShotHeight = 8f; // â Increased from 5f
    private const float InitialSpeedBoost = 1.12f; // â NEW - faster shots

    private ulong timeSinceShot;
    private GameEvents gameEvents;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        SetBallAnimationFromVelocity();

        if (sprite != null)
            sprite.Scale = new Vector2(sprite.Scale.X, 0.8f);

        // â Apply speed boost for shot power feel
        ball.Velocity *= InitialSpeedBoost;
        ball.Height = ShotHeight;

        // â Add shooting accuracy variance
        if (ball.Carrier != null)
        {
            float shootingSkill = ball.Carrier.GameAttributes.Shooting;
            float accuracy = Mathf.InverseLerp(50f, 99f, shootingSkill);

            // Poor shooters: up to 8Â° deviation, good shooters: ~1Â°
            float maxDeviation = Mathf.Lerp(8f, 1f, accuracy);
            float angleError = (GD.Randf() * 2f - 1f) * maxDeviation;
            ball.Velocity = ball.Velocity.Rotated(Mathf.DegToRad(angleError));
        }

        timeSinceShot = Time.GetTicksMsec();

        if (shotParticles != null)
            shotParticles.Emitting = true;

        gameEvents.EmitImpact(ball.Position, false);
        soundPlayer.Play(SoundPlayer.Sound.POWERSHOT);
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - timeSinceShot > DurationShot)
        {
            TransitionState(Ball.State.FREEFORM);
        }
        else
        {
            // â Apply air friction during shot
            float airFriction = 35f;
            ball.Velocity = ball.Velocity.MoveToward(Vector2.Zero, airFriction * (float)delta);

            MoveAndBounce((float)delta);
        }
    }

    public override void _ExitTree()
    {
        if (sprite != null)
            sprite.Scale = new Vector2(sprite.Scale.X, 1f);

        if (shotParticles != null)
            shotParticles.Emitting = false;
    }
}