using Godot;
using System;
public partial class BallStateCarried : BallState
{
    private const float DribbleFrequency = 10.0f;
    private const float DribbleIntensity = 3.0f;

    // X = forward distance in front of feet, Y = static height offset
    private static readonly Vector2 OffsetFromPlayer = new Vector2(10, 4);

    private float dribbleTime = 0.0f;
    public GameEvents gameEvents;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.EmitSignal("BallPossessed", carrier.playerID);
    }

    public override void _Process(double delta)
    {
        dribbleTime += (float)delta;

        Vector2 heading = carrier.heading;
        if (heading != Vector2.Zero)
            heading = heading.Normalized();

        // heading *= -1f;

        float wobble = 0f;
        if (carrier.Velocity != Vector2.Zero)
        {
            wobble = Mathf.Cos(dribbleTime * DribbleFrequency) * DribbleIntensity;

            if (heading.X >= 0)
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

        // Get 8-direction string from carrier heading
        // Final carried-ball position
        string dir = GetDirectionString(carrier.heading);
        Vector2 forward = GetForwardOffset(dir);

        // ✔️ FIX: wobble perpendicular to actual heading, not forward offset
        Vector2 perp = new Vector2(-heading.Y, heading.X).Normalized();
        wobble = Mathf.Cos(dribbleTime * DribbleFrequency) * DribbleIntensity;

        ball.Position =
            carrier.Position
            + forward
            + perp * wobble
            + new Vector2(0, OffsetFromPlayer.Y);
    }

    private Vector2 GetForwardOffset(string dir)
    {
        float dist = OffsetFromPlayer.X;
        float diag = dist * 0.6071f;

        return dir switch
        {
            "north" => new Vector2(0, -dist),
            "south" => new Vector2(0, dist * 0.6071f),
            "east" => new Vector2(dist, 0),
            "west" => new Vector2(-dist, 0),

            "northeast" => new Vector2(diag, -diag),
            "northwest" => new Vector2(-diag, -diag),
            "southeast" => new Vector2(diag, diag),
            "southwest" => new Vector2(-diag, diag),

            _ => new Vector2(0, dist)
        };
    }

    private Vector2 GetWobbleVector(string dir)
    {
        return dir switch
        {
            // Cardinal directions use standard cross-axis wobble
            "north" or "south" => new Vector2(1, 0),  // Wobble left/right across feet
            "east" or "west" => new Vector2(0, 1),  // Wobble up/down across feet

            // Diagonal directions use horizontal-dominant wobble to avoid pulling backward
            "southeast" or "southwest" => new Vector2(1, 0.2f),
            "northeast" or "northwest" => new Vector2(1, -0.2f),

            _ => new Vector2(1, 0)
        };
    }

    private string GetDirectionString(Vector2 dir)
    {
        if (dir == Vector2.Zero)
            return "south";

        float angleDeg = Mathf.RadToDeg(dir.Angle());
        int snappedAngle = (int)(Mathf.Round(angleDeg / 45f) * 45f);
        if (snappedAngle == -180) snappedAngle = 180;

        return snappedAngle switch
        {
            0 => "east",
            45 => "southeast",
            90 => "south",
            135 => "southwest",
            180 => "west",
            -135 => "northwest",
            -90 => "north",
            -45 => "northeast",
            _ => "south"
        };
    }

    public override void _ExitTree()
    {
        gameEvents.EmitSignal("BallReleased");
    }
}