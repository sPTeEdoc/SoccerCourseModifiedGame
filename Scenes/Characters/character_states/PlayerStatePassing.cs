using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class PlayerStatePassing : PlayerState
{
    public SoundPlayer soundPlayer;
    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        animationPlayer.Play("kick");
        player.Velocity = Vector2.Zero;
        soundPlayer.Play(SoundPlayer.Sound.PASS);
    }

    public override void OnAnimationComplete()
    {
        PlayerCharacter passTarget = stateData.PassTarget ?? FindTeammateInView();

        if (passTarget == null)
        {
            ball.PassTo(ball.Position + player.heading * player.speed, receiver: passTarget);
        }
        else
        {
            Vector2 direction = player.Position.DirectionTo(passTarget.Position);

            if (Math.Sign(player.heading.X) != Math.Sign(direction.X))
            {
                player.heading *= -1;
            }

            ball.PassTo(passTarget.Position + passTarget.Velocity * 0.8f, receiver: passTarget);
        }

        TransitionState(PlayerCharacter.State.MOVING);
    }

    private PlayerCharacter FindTeammateInView()
    {
        var playersInView = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player && p.teamID == player.teamID)
            .ToList();

        playersInView.Sort((p1, p2) =>
            p1.Position.DistanceSquaredTo(player.Position)
            .CompareTo(p2.Position.DistanceSquaredTo(player.Position)));

        return playersInView.Count > 0 ? playersInView[0] : null;
    }
}