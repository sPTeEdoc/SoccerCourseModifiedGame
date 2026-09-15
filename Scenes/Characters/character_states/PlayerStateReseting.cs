using Godot;
using System;

[GlobalClass]
public partial class PlayerStateReseting : PlayerState
{
    private bool hasArrived = false;

    public GameEvents gameEvents;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.KickoffStarted += OnKickoffStarted;
        player.GameAttributes.Speed = 90; // just for getting to their spots at a reasonable speed
    }

    public override void _Process(double delta)
    {
        if (!hasArrived)
        {
            Vector2 direction = player.Position.DirectionTo(stateData.ResetPosition);
            if (player.Position.DistanceSquaredTo(stateData.ResetPosition) < 2)
            {
                player.GameAttributes.Speed = player.TrueAttributes.Speed;
                hasArrived = true;
                player.Velocity = Vector2.Zero;
                player.FaceTowardsTargetGoal();
                if (player.targetGoal.IsNorth)
                {
                    if (player.TeamIsKickingOff) player.SetBufferedDirection(Vector2.Down);
                    else player.SetBufferedDirection(Vector2.Up);
                }
                else
                {
                    if (player.TeamIsKickingOff) player.SetBufferedDirection(Vector2.Up);
                    else player.SetBufferedDirection(Vector2.Down);
                }
            }
            else
            {
                player.Velocity = direction * player.GameAttributes.Speed;
            }

            player.SetMovementAnimation();
            player.SetHeading();
        }
    }

    public override bool IsReadyForKickoff() => hasArrived;

    private void OnKickoffStarted()
    {
        TransitionState(PlayerCharacter.State.MOVING);
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.KickoffStarted -= OnKickoffStarted;
    }
}