using Godot;
using System;

[GlobalClass]
public partial class PlayerStateTackling : PlayerState
{
    private const float GROUND_FRICTION = 250.0f;
    private const int DURATION_PRIOR_RECOVERY = 500; // Increased penalty window for missing (500ms)

    private bool isTackleComplete = false;
    private bool hasConnectedWithBall = false;
    private bool hasRecovered = false;
    private int timeFinishTackle = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        isTackleComplete = false;
        hasConnectedWithBall = false;

        animationPlayer.Play($"{player.AnimPrefix}tackle");
        tackleDamageEmitterArea.Monitoring = true;

        // FORCE THE SLIDE: If they are moving, give them a lunging velocity burst along their heading!
        Vector2 slideDirection = player.heading != Vector2.Zero ? player.heading.Normalized() : Vector2.Right;
        player.Velocity = slideDirection * (player.speed * 1.6f);

        // Track the exact time the lunge started
        timeFinishTackle = (int)Time.GetTicksMsec();
    }

    public override void _Process(double delta)
    {
        int timeElapsed = (int)Time.GetTicksMsec() - timeFinishTackle;

        // 1. Proximity Check with a micro-delay (Must be sliding for at least 100ms before connecting)
        if (!hasConnectedWithBall && ball.Carrier != null && ball.Carrier.teamID != player.teamID && !isTackleComplete)
        {
            if (timeElapsed > 100 && player.Position.DistanceTo(ball.Position) < 20f)
            {
                ExecuteSuccessfulTackle();
                return;
            }
        }

        // 2. Physical slide momentum management
        if (!isTackleComplete)
        {
            player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * GROUND_FRICTION);

            if (player.Velocity == Vector2.Zero)
            {
                isTackleComplete = true;
                timeFinishTackle = (int)Time.GetTicksMsec(); // Reset timer to mark recovery start

                if (!hasConnectedWithBall)
                {
                    if (!hasRecovered)
                        TransitionState(PlayerCharacter.State.RECOVERING);
                    hasRecovered = true;
                }
            }
        }
        // 3. Penalty recovery window if they missed
        else if ((int)Time.GetTicksMsec() - timeFinishTackle > DURATION_PRIOR_RECOVERY)
        {
            if (!hasRecovered)
                TransitionState(PlayerCharacter.State.RECOVERING);
        }
    }

    private void ExecuteSuccessfulTackle()
    {
        hasConnectedWithBall = true;

        // 1. Tell the opponent explicitly they no longer own this ball
        if (ball.Carrier != null)
        {
            var oldCarrier = ball.Carrier;
            // Force the old carrier into a brief "stumble" or "stagger" state if you have one
            // oldCarrier.SwitchState(PlayerCharacter.State.STUMBLING);
        }

        // 2. Clear the carrier ref completely BEFORE applying physics
        ball.Carrier = null;

        // 3. Push the ball slightly outside the player's immediate collision circle 
        // so it doesn't instantly re-trigger an overlap collection
        Vector2 tacklePopDirection = player.heading != Vector2.Zero ? player.heading.Normalized() : Vector2.Up;
        ball.Position += tacklePopDirection * 5f;

        // 4. Fire it away
        ball.Velocity = tacklePopDirection * 250f; // Crank up the force to break away from running hitboxes
        ball.SwitchState(Ball.State.FREEFORM, BallStateData.Build().SetLockDuration(200));

        player.Velocity = Vector2.Zero;
        isTackleComplete = true;

        TransitionState(PlayerCharacter.State.MOVING);
    }

    public override void _ExitTree()
    {
        tackleDamageEmitterArea.Monitoring = false;
    }
}