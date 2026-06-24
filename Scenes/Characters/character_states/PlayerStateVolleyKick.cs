using Godot;
using System;

[GlobalClass]
public partial class PlayerStateVolleyKick : PlayerState
{
    private const float BALL_HEIGHT_MIN = 1.0f;
    private const float BALL_HEIGHT_MAX = 25.0f;
    private const float BONUS_POWER = 1.5f;

    public SoundPlayer soundPlayer;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        animationPlayer.Play($"{player.AnimPrefix}volley_kick");
        ballDetectionArea.BodyEntered += OnBallEntered;
    }

    public override void _ExitTree()
    {
        if (ballDetectionArea != null)
            ballDetectionArea.BodyEntered -= OnBallEntered;
    }


    private void OnBallEntered(Node body)
    {
        if (body is Ball contactBall &&
            contactBall.CanAirConnect(BALL_HEIGHT_MIN, BALL_HEIGHT_MAX))
        {
            Vector2 destination = targetGoal.GetRandomTargetPosition();
            Vector2 direction = ball.Position.DirectionTo(destination);

            soundPlayer.Play(SoundPlayer.Sound.POWERSHOT);
            contactBall.Shoot(direction * player.power * BONUS_POWER);
        }
    }

    public override void OnAnimationComplete()
    {
        TransitionState(PlayerCharacter.State.RECOVERING);
    }
}