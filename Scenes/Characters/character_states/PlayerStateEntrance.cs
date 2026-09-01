using Godot;
using System;

[GlobalClass]
public partial class PlayerStateEntrance : PlayerState
{
    private bool hasArrived = false;

    public override void _Process(double delta)
    {
        if (!hasArrived)
        {
            Vector2 direction = player.Position.DirectionTo(stateData.EntrancePosition);

            if (player.Position.DistanceSquaredTo(stateData.EntrancePosition) < 4f)
            {
                hasArrived = true;
                player.Velocity = Vector2.Zero;

                // Stop movement animations
                player.SetMovementAnimation();

                // 1. Force the physical and visual heading to look East (Vector2.Right)
                player.heading = Vector2.Right; 
                player.SetBufferedDirection(Vector2.Right);
                player.SetMovementAnimation(); // Refresh animation to reflect the new direction
            }
            else
            {
                player.Velocity = direction * player.speed;
                player.SetMovementAnimation();
            }
        }
        else
        {
            // Ensure they maintain zero velocity and continue facing East while waiting
            player.Velocity = Vector2.Zero;
            player.SetBufferedDirection(Vector2.Right);
            player.SetMovementAnimation();
        }
    }

    // This makes sure ArenaActorsContainer knows this specific player is done entering
    public override bool IsReadyToGoToKickoffSpots() => hasArrived;
}