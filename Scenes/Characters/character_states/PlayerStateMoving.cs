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
        player.Velocity = direction * player.speed;

        if (player.Velocity != Vector2.Zero)
        {
            teammateDetectionArea.Rotation = player.Velocity.Angle();
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
        return player.role != PlayerCharacter.Role.GOALIE;
    }

    private bool CanTeammatePassBall()
    {
        return ball.Carrier != null &&
               ball.Carrier.team == player.team &&
               ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.CPU;
    }

    public override bool CanPass()
    {
        return true;
    }
}