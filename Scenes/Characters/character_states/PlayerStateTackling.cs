using Godot;
using System;

[GlobalClass]
public partial class PlayerStateTackling : PlayerState
{
    private const float GROUND_FRICTION = 350.0f;
    private const float TACKLE_LUNGE_FACTOR = 1.35f; // Lunge speed relative to base speed
    private const int DURATION_PRIOR_RECOVERY = 200;

    private bool isTackleComplete = false;
    private int timeFinishTackle = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        tackleDamageEmitterArea.Monitoring = true;

        // 1. Determine tackle direction: prefer pointing toward ball carrier if within range
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

        // 2. Update player heading to match full 2D direction
        player.heading = tackleDir;

        // 3. Apply initial tackle lunge impulse toward target
        player.Velocity = tackleDir * (player.speed * TACKLE_LUNGE_FACTOR);

        // 4. Play corresponding 8-directional animation
        string directionStr = GetDirectionString(tackleDir);
        player.animatedSprite2D.Play("tackle_" + directionStr);
    }

    public override void _Process(double delta)
    {
        if (!isTackleComplete)
        {
            // Decelerate during the tackle slide
            player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * GROUND_FRICTION);
            if (player.Velocity == Vector2.Zero)
            {
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