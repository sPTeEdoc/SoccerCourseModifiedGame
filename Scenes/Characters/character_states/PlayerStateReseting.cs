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
    }

    public override void _Process(double delta)
    {
        if (!hasArrived)
        {
            Vector2 direction = player.Position.DirectionTo(stateData.ResetPosition);
            if (player.Position.DistanceSquaredTo(stateData.ResetPosition) < 2)
            {
                hasArrived = true;
                player.Velocity = Vector2.Zero;
                player.FaceTowardsTargetGoal();
            }
            else
            {
                player.Velocity = direction * player.speed;
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