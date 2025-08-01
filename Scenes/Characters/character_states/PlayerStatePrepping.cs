using Godot;
using System;

[GlobalClass]
public partial class PlayerStatePreppingShot : PlayerState
{
    private const float DURATION_MAX_BONUS = 1000.0f;
    private const float EASE_REWARD_FACTOR = 2.0f;

    private Vector2 shotDirection = Vector2.Zero;
    private int timeStartShot = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        animationPlayer.Play("prep_kick");
        player.Velocity = Vector2.Zero;
        timeStartShot = (int)Time.GetTicksMsec();
        shotDirection = player.heading;
    }

    public override void _Process(double delta)
    {
        shotDirection += KeyUtils.GetInputVector(player.controlScheme) * (float)delta;

        if (KeyUtils.IsActionJustReleased(player.controlScheme, KeyUtils.Action.SHOOT))
        {
            float durationPress = Mathf.Clamp((int)Time.GetTicksMsec() - timeStartShot, 0f, DURATION_MAX_BONUS);
            float easeTime = durationPress / DURATION_MAX_BONUS;
            float bonus = Ease(easeTime, EASE_REWARD_FACTOR);
            float shotPower = player.power * (1 + bonus);

            shotDirection = shotDirection.Normalized();

            var data = PlayerStateData.Build()
                .SetShotPower(shotPower)
                .SetShotDirection(shotDirection);

            TransitionState(PlayerCharacter.State.SHOOTING, data);
        }
    }

    private float Ease(float t, float factor)
    {
        return Mathf.Pow(t, factor); // Simple ease-in for power buildup
    }

    public override bool CanPass() => true;
}