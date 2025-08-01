using Godot;
using System;

[GlobalClass]
public partial class PlayerStateHurt : PlayerState
{
    private const float AIR_FRICTION = 35.0f;
    private const float BALL_TUMBLE_SPEED = 100.0f;
    private const int DURATION_HURT = 1000;
    private const float HURT_HEIGHT_VELOCITY = 3.0f;

    public SoundPlayer soundPlayer;
    public GameEvents gameEvents;

    private int timeStartHurt = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        animationPlayer.Play("hurt");
        timeStartHurt = (int)Time.GetTicksMsec();
        player.heightVelocity = HURT_HEIGHT_VELOCITY;
        player.height = 0.1f;

        if (ball.Carrier == player)
        {
            ball.Tumble(stateData.HurtDirection * BALL_TUMBLE_SPEED);
            soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
            soundPlayer.Play(SoundPlayer.Sound.HURT);
            gameEvents = GetNode<GameEvents>("/root/GameEvents");
            gameEvents.EmitImpact(player.Position, false);
        }
    }

    public override void _Process(double delta)
    {
        if ((int)Time.GetTicksMsec() - timeStartHurt > DURATION_HURT)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }

        player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * AIR_FRICTION);
    }
}