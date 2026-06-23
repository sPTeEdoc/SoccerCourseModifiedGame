using Godot;
using System;

[GlobalClass]
public partial class PlayerStateMoving : PlayerState
{
    public override void _Process(double delta)
    {
        if (player.controlScheme == PlayerCharacter.ControlScheme.CPU)
        {
            aiBehavior.ProcessAI();
        }
        else
        {
            HandleHumanMovement();
        }

        player.SetMovementAnimation();
        player.SetHeading();
    }

    private void HandleHumanMovement()
    {
        Vector2 direction = KeyUtils.GetInputVector(player.controlScheme);

        // Determine speed based on whether they are sprinting
        bool isSprinting = KeyUtils.IsActionPressed(player.controlScheme, KeyUtils.Action.SPRINT);
        float currentSpeed = isSprinting ? player.speed * 1.8f : player.speed;

        player.Velocity = direction * currentSpeed;

        if (player.Velocity != Vector2.Zero)
        {
            teammateDetectionArea.Rotation = player.Velocity.Angle();

            // SPRINT LOGIC: If carrying the ball, knock it ahead!
            if (player.HasBall() && isSprinting)
            {
                // Knock it ahead 35-40 pixels into a short freeform state
                Vector2 pushVelocity = direction * (currentSpeed * 1.3f);

                // Release carrier temporarily so a defender can step in
                ball.Velocity = pushVelocity;
                ball.Carrier = null;

                // Lock the ball for a tiny window (~150ms) so the user doesn't instantly recapture it
                ball.SwitchState(Ball.State.FREEFORM, BallStateData.Build().SetLockDuration(150));
            }
        }

        if (KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.PASS))
        {
            if (player.HasBall())
            {
                TransitionState(PlayerCharacter.State.PASSING);
            }
            else if (CanTeammatePassBall())
            {
                ball.Carrier?.GetPassRequest(player);
            }
            else
            {
                player.EmitSwapRequest(player);
            }
        }
        else if (KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.SHOOT))
        {
            if (player.HasBall())
            {
                TransitionState(PlayerCharacter.State.PREPPING_SHOT);
            }
            else if (ball.CanAirInteract())
            {
                if (player.Velocity == Vector2.Zero)
                {
                    if (player.IsFacingTargetGoal())
                    {
                        TransitionState(PlayerCharacter.State.VOLLEY_KICK);
                    }
                    else
                    {
                        TransitionState(PlayerCharacter.State.BICYCLE_KICK);
                    }
                }
                else
                {
                    TransitionState(PlayerCharacter.State.HEADER);
                }
            }
            else if (player.Velocity != Vector2.Zero)
            {
                TransitionState(PlayerCharacter.State.TACKLING);
            }
        }
    }

    public override bool CanCarryBall()
    {
        // Goalies can now absolutely possess and carry the ball!
        return true;
    }

    private bool CanTeammatePassBall()
    {
        return ball.Carrier != null &&
               ball.Carrier.teamID == player.teamID &&
               ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.CPU;
    }

    public override bool CanPass()
    {
        return true;
    }
}