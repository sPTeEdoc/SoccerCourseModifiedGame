using Godot;
using System;

public partial class BallStateShot : BallState
{
    private const int DurationShot = 1000;
    private const float ShotHeight = 5f;
    private const float ShotSpriteScale = 0.8f;

    private ulong timeSinceShot;
    private GameEvents gameEvents;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        SetBallAnimationFromVelocity();

        if (sprite != null)
            sprite.Scale = new Vector2(sprite.Scale.X, ShotSpriteScale);

        ball.Height = ShotHeight;
        timeSinceShot = Time.GetTicksMsec();

        if (shotParticles != null)
            shotParticles.Emitting = true;

        gameEvents.EmitImpact(ball.Position, false);
    }

    public override void _Process(double delta)
    {
        
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (Time.GetTicksMsec() - timeSinceShot > DurationShot)
        {
            TransitionState(Ball.State.FREEFORM);
        }
        else
        {
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