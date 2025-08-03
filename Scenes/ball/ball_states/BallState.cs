using Godot;
using System;

public partial class BallState : Node
{
    public const float Gravity = 10f;

    [Signal]
    public delegate void StateTransitionRequestedEventHandler(Ball.State newState, BallStateData data);


    protected AnimationPlayer animationPlayer = null;
    protected Ball ball = null;
    protected PlayerCharacter carrier = null;
    protected Area2D playerDetectionArea = null;
    protected GpuParticles2D shotParticles = null;
    protected Sprite2D sprite = null;
    protected BallStateData stateData = null;

    protected SoundPlayer soundPlayer;

    public override void _Ready()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
    }


    public void Setup(
        Ball contextBall,
        BallStateData contextStateData,
        Area2D contextPlayerDetectionArea,
        PlayerCharacter contextCarrier,
        AnimationPlayer contextAnimationPlayer,
        Sprite2D contextSprite,
        GpuParticles2D contextShotParticles
    )
    {
        ball = contextBall;
        playerDetectionArea = contextPlayerDetectionArea;
        carrier = contextCarrier;
        animationPlayer = contextAnimationPlayer;
        sprite = contextSprite;
        stateData = contextStateData;
        shotParticles = contextShotParticles;
        if (ball.Carrier != null)
            ball.Carrier.EmitSwapRequest(ball.Carrier);
    }

    protected void TransitionState(Ball.State newState, BallStateData data = null)
    {
        EmitSignal("StateTransitionRequested", (int)newState, data ?? new BallStateData());
    }

    protected void SetBallAnimationFromVelocity()
    {
        if (ball.Velocity == Vector2.Zero)
        {
            animationPlayer.Play("idle");
        }
        else if (ball.Velocity.X > 0)
        {
            animationPlayer.Play("roll");
            animationPlayer.Advance(0f);
        }
        else
        {
            animationPlayer.PlayBackwards("roll");
            animationPlayer.Advance(0f);
        }
    }

    protected void ProcessGravity(float delta, float bounciness = 0f)
    {
        if (ball.Height > 0 || ball.HeightVelocity > 0)
        {
            ball.HeightVelocity -= Gravity * delta;
            ball.Height += ball.HeightVelocity;

            if (ball.Height < 0)
            {
                ball.Height = 0;
                if (bounciness > 0 && ball.HeightVelocity < 0)
                {
                    ball.HeightVelocity = -ball.HeightVelocity * bounciness;
                    ball.Velocity *= bounciness;
                }
            }
        }
    }

    protected void MoveAndBounce(float delta)
    {
        var collision = ball.MoveAndCollide(ball.Velocity * delta);
        if (collision != null)
        {
            ball.Velocity = ball.Velocity.Bounce(collision.GetNormal()) * Ball.BOUNCINESS;
            soundPlayer.Play(SoundPlayer.Sound.BOUNCE);
            ball.SwitchState(Ball.State.FREEFORM);
        }
    }

    public virtual bool CanAirInteract()
    {
        return false;
    }
}