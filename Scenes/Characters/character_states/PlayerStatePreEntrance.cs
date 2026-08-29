using Godot;
using System;

[GlobalClass]
public partial class PlayerStatePreEntrance : PlayerState
{
    private bool hasArrived = false;

    public GameEvents gameEvents;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.KickoffStarted += OnPreEntranceFinished;
    }

    public override async void _Process(double delta)
    {
        if (!hasArrived)
        {
            Vector2 direction = player.Position.DirectionTo(stateData.PreEntrancePosition);

            if (player.Position.DistanceSquaredTo(stateData.PreEntrancePosition) < 4)
            {
                hasArrived = true;
                player.Velocity = Vector2.Zero;
                player.FaceTowardsTargetGoal();

                // Immediately transition to ENTRANCE state
                TransitionState(PlayerCharacter.State.ENTRANCE,
                    PlayerStateData.Build().SetEntrancePosition(player.entrancePosition));

                return;
            }

            player.Velocity = direction * player.speed;
        }

        player.SetMovementAnimation();
        player.SetHeading();
    }

    public override bool IsReadyForEntrance() => hasArrived;

    private void OnPreEntranceFinished()
    {
        TransitionState(PlayerCharacter.State.ENTRANCE);
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.KickoffStarted -= OnPreEntranceFinished;
    }
}