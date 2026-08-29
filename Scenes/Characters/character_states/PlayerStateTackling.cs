using Godot;
using System;

[GlobalClass]
public partial class PlayerStateTackling : PlayerState
{
    // Tuning Parameters
    private const float TARGET_SLIDE_DISTANCE = 25.0f; // Distance in pixels to slide
    private const float GROUND_FRICTION = 450.0f;       // Deceleration rate (px/s^2)
    private const int DURATION_PRIOR_RECOVERY = 150;    // Milliseconds paused after slide ends

    private bool isTackleComplete = false;
    private int timeFinishTackle = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        tackleDamageEmitterArea.Monitoring = true;

        // 1. Determine tackle direction vector (towards ball carrier if valid, otherwise current heading/velocity)
        Vector2 tackleDir = player.heading;

        if (ball != null && ball.Carrier != null && ball.Carrier != player)
        {
            Vector2 toCarrier = player.Position.DirectionTo(ball.Carrier.Position);
            if (toCarrier != Vector2.Zero)
            {
                tackleDir = toCarrier;
            }
        }
        else if (player.Velocity.LengthSquared() > 1.0f)
        {
            tackleDir = player.Velocity.Normalized();
        }

        player.heading = tackleDir;

        // 2. Calculate exact launch speed needed to travel TARGET_SLIDE_DISTANCE under GROUND_FRICTION
        float initialSlideSpeed = Mathf.Sqrt(2.0f * GROUND_FRICTION * TARGET_SLIDE_DISTANCE);
        player.Velocity = tackleDir * initialSlideSpeed;

        // 3. Play matching 8-directional animation
        string directionStr = GetDirectionString(tackleDir);
        player.animatedSprite2D.Play("tackle_" + directionStr);
    }

    public override void _Process(double delta)
    {
        if (!isTackleComplete)
        {
            // Uniform deceleration across grass surface
            player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * GROUND_FRICTION);

            if (player.Velocity.LengthSquared() < 1.0f)
            {
                player.Velocity = Vector2.Zero;
                isTackleComplete = true;
                timeFinishTackle = (int)Time.GetTicksMsec();
            }
        }
        else if ((int)Time.GetTicksMsec() - timeFinishTackle > DURATION_PRIOR_RECOVERY)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
    }

    public override void _ExitTree()
    {
        tackleDamageEmitterArea.Monitoring = false;
    }

    private string GetDirectionString(Vector2 dir)
    {
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
}