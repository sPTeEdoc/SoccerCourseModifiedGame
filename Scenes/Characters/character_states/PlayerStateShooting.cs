using Godot;
using System;

[GlobalClass]
public partial class PlayerStateShooting : PlayerState
{
    public SoundPlayer soundPlayer;
    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");

        // Snap the visual rendering angle for your sprite selection
        float snappedAngle = Mathf.Round(player.heading.Angle() * 180f / MathF.PI / 45f) * 45f;
        int angleCheck = (int)snappedAngle;
        if (angleCheck == -180) angleCheck = 180;

        string animPrefix = "kick_";
        string directionStr = "south";

        if (angleCheck == 0) directionStr = "east";
        else if (angleCheck == -45) directionStr = "northeast";
        else if (angleCheck == -90) directionStr = "north";
        else if (angleCheck == -135) directionStr = "northwest";
        else if (angleCheck == 180) directionStr = "west";
        else if (angleCheck == 135) directionStr = "southwest";
        else if (angleCheck == 90) directionStr = "south";
        else if (angleCheck == 45) directionStr = "southeast";

        player.animatedSprite2D.Play(animPrefix + directionStr);
        // animationPlayer.Play("kick");

        OnAnimationComplete();
    }

    public override void OnAnimationComplete()
    {
        if (player.controlScheme == PlayerCharacter.ControlScheme.CPU)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
        else
        {
            TransitionState(PlayerCharacter.State.MOVING);
        }

        ShootBall();
    }

    private void ShootBall()
    {
        soundPlayer.Play(SoundPlayer.Sound.SHOT);
        ball.Shoot(stateData.ShotDirection * stateData.ShotPower);
    }
}