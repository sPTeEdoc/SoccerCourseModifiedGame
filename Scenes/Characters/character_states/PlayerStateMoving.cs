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
        bool isPressing = KeyUtils.IsActionPressed(player.controlScheme, KeyUtils.Action.AButton);
        Vector2 direction = Vector2.Zero;

        if (isPressing && ball.Carrier != null && ball.Carrier.teamID != player.teamID)
        {
            // PES AUTOMATED PRESS: Guide the player directly toward the ball carrier
            direction = player.Position.DirectionTo(ball.Position);

            // Automatically execute a standing tackle if the press gets them within range
            if (player.Position.DistanceTo(ball.Position) < 24f)
            {
                TransitionState(PlayerCharacter.State.STANDING_TACKLE);
                return;
            }
        }
        else
        {
            // Standard manual stick movement
            direction = KeyUtils.GetInputVector(player.controlScheme);
        }

        // Determine speed based on whether they are sprinting
        bool isSprinting = KeyUtils.IsActionPressed(player.controlScheme, KeyUtils.Action.R1Button);
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

        if (KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.AButton))
        {
            if (player.HasBall())
            {
                TransitionState(PlayerCharacter.State.PASSING);
            }
            else if (CanTeammatePassBall())
            {
                ball.Carrier?.GetPassRequest(player);
            }
        }
        else if (KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.L1Button))
        {
            player.EmitSwapRequest(player);
        }
        else if ((KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.XButton)))
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
        }
        else if (KeyUtils.IsActionJustPressed(player.controlScheme, KeyUtils.Action.BButton))
        {
            if (!player.HasBall())
            {
                TransitionState(PlayerCharacter.State.TACKLING);
            }
            // else if (player.Velocity != Vector2.Zero)
            // {
            //     TransitionState(PlayerCharacter.State.TACKLING);
            // // Check if the human player is moving fast enough to warrant a slide
            // // Let's say if they are moving at more than 70% of baseline speed
            // if (player.Velocity.Length() > player.speed * 0.7f)
            // {
            //     // High risk / High reward slide lunge!
            //     TransitionState(PlayerCharacter.State.TACKLING);
            // }
            // else
            // {
            //     // Controlled, quick standing challenge / poke tackle
            //     TransitionState(PlayerCharacter.State.STANDING_TACKLE);
            // }
            // }
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