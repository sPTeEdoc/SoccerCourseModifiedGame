using Godot;
using System;

[GlobalClass]
public partial class PlayerStateHeader : PlayerState
{
    private const float BALL_HEIGHT_MIN = 5.0f;
    private const float BALL_HEIGHT_MAX = 30.0f;
    private const float BONUS_POWER = 1.3f;
    private const float HEIGHT_START = 0.1f;
    private const float HEIGHT_VELOCITY = 1.5f;

    private SoundPlayer soundPlayer;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        animationPlayer.Play("header");
        player.height = HEIGHT_START;
        player.heightVelocity = HEIGHT_VELOCITY;

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
            soundPlayer.Play(SoundPlayer.Sound.POWERSHOT);
            Vector2 destination = player.targetGoal.GetRandomTargetPosition();
            Vector2 direction = contactBall.Position.DirectionTo(destination);
            contactBall.Shoot(direction * player.power * BONUS_POWER);
        }
    }

    public override void _Process(double delta)
    {
        if (player.height == 0)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
    }
}