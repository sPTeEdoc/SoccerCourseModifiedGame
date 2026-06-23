using Godot;
using System;

[GlobalClass]
public partial class PlayerStateShooting : PlayerState
{
    public SoundPlayer soundPlayer;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        animationPlayer.Play($"{player.AnimPrefix}kick");
        
        // Completely stop player vector on the frame actual physical contact is made
        player.Velocity = Vector2.Zero;

        // INSTANT RELEASE: Strike the ball the exact frame we transition here
        ShootBall();
    }

    public override void OnAnimationComplete()
    {
        // Strictly handles cleanup and stance recovery now
        if (player.controlScheme == PlayerCharacter.ControlScheme.CPU)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
        else
        {
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }

    private void ShootBall()
    {
        if (stateData != null)
        {
            soundPlayer.Play(SoundPlayer.Sound.SHOT);
            ball.Shoot(stateData.ShotDirection * stateData.ShotPower);

            if (player.controlScheme == PlayerCharacter.ControlScheme.CPU)
                return;
            
            // Optional: If the ball was overcharged from the prep state, give it an upward trajectory arc
            if (stateData.ShotPower < player.power * 1.2f && KeyUtils.IsActionPressed(player.controlScheme, KeyUtils.Action.SHOOT))
            {
               // You can feed vertical lift variables straight into ball.HeightVelocity here if needed!
            }
        }
    }
}