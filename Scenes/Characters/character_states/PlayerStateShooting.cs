using Godot;
using System;

[GlobalClass]
public partial class PlayerStateShooting : PlayerState
{
    public SoundPlayer soundPlayer;
    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        animationPlayer.Play("kick");
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