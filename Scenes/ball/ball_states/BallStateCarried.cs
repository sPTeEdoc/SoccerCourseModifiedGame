using FunnyOldGame;
using Godot;
using System;

public partial class BallStateCarried : BallState
{
    private const float DribbleFrequency = 10.0f;
    private const float DribbleIntensity = 3.0f;
    private static readonly Vector2 OffsetFromPlayer = new Vector2(10, 4);

    private float dribbleTime = 0.0f;
    public GameEvents gameEvents;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.EmitSignal("BallPossessed", carrier.fullname);
    }

    public override void _Process(double delta)
    {
        float vx = 0.0f;
        dribbleTime += (float)delta;

        if (carrier.Velocity != Vector2.Zero)
        {
            if (carrier.Velocity.X != 0)
                vx = Mathf.Cos(dribbleTime * DribbleFrequency) * DribbleIntensity;

            if (carrier.heading.X >= 0)
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
        else
        {
            animationPlayer.Play("idle");
        }

        ProcessGravity((float)delta);
        ball.Position = carrier.Position + new Vector2(vx + carrier.heading.X * OffsetFromPlayer.X, OffsetFromPlayer.Y);
    }

    public override void _ExitTree()
    {
        gameEvents.EmitSignal("BallReleased");
    }
}
