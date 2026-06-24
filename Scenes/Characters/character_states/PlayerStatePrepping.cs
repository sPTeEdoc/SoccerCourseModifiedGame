using Godot;
using System;

[GlobalClass]
public partial class PlayerStatePreppingShot : PlayerState
{
    private const float DURATION_MAX_BONUS = 400.0f; // Lowered slightly for snappier max charge (~0.8s)
    private const float DURATION_FORCE_RELEASE = 400.0f; // Capped at 1 second absolute max
    private const float EASE_REWARD_FACTOR = 2.0f;

    private Vector2 shotDirection = Vector2.Zero;
    private int timeStartShot;

    public override void _EnterTree()
    {
        animationPlayer.Play($"{player.AnimPrefix}prep_kick");

        // PES style: Keep a tiny bit of dribbling momentum so they don't halt like a statue
        player.Velocity *= 0.4f;

        timeStartShot = (int)Time.GetTicksMsec();
        shotDirection = player.heading;
    }

    public override void _Process(double delta)
    {
        int elapsed = (int)Time.GetTicksMsec() - timeStartShot;

        if (KeyUtils.IsActionJustReleased(player.controlScheme, KeyUtils.Action.XButton) || elapsed >= DURATION_FORCE_RELEASE)
        {
            Vector2 goalCenter = player.targetGoal.GetCenterTargetPosition();
            Vector2 baseDirToGoal = player.Position.DirectionTo(goalCenter);
            Vector2 manualStickInput = KeyUtils.GetInputVector(player.controlScheme);

            if (manualStickInput != Vector2.Zero)
            {
                // Blend: 50% goal targeting, 50% manual stick skewing
                shotDirection = (baseDirToGoal * 0.7f + manualStickInput * 0.3f).Normalized();
            }
            else
            {
                // No stick input means a clean strike down the center line
                shotDirection = baseDirToGoal;
            }

            FireShot(elapsed, elapsed >= DURATION_FORCE_RELEASE);
        }
    }

    private void FireShot(int elapsed, bool isOvercharged)
    {
        float durationPress = Mathf.Clamp(elapsed, 0f, DURATION_MAX_BONUS);
        float easeTime = durationPress / DURATION_MAX_BONUS;
        float bonus = Ease(easeTime, EASE_REWARD_FACTOR);

        float shotPower = player.power * (1.2f + bonus);
        shotDirection = shotDirection.Normalized();

        // Penalty System if they exceeded the meter limits
        if (isOvercharged)
        {
            // Sky the ball: Malus reduction to power, random dispersion to direction
            shotPower *= 0.8f;
            float randomDeviation = (float)GD.RandRange(-0.4f, 0.4f);
            shotDirection = shotDirection.Rotated(randomDeviation);
        }

        var data = PlayerStateData.Build()
            .SetShotPower(shotPower)
            .SetShotDirection(shotDirection);

        // Package data and pass it right along
        TransitionState(PlayerCharacter.State.SHOOTING, data);
    }

    private float Ease(float t, float factor)
    {
        return Mathf.Pow(t, factor);
    }

    public override bool CanPass() => true;
}