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
        // animationPlayer.Play("kick");
        player.Velocity = Vector2.Zero;
        soundPlayer.Play(SoundPlayer.Sound.PASS);

        // Snap the visual rendering angle for your sprite selection
        float snappedAngle = Mathf.Round(player.heading.Angle() * 180f / MathF.PI / 45f) * 45f;
        int angleCheck = (int)snappedAngle;
        if (angleCheck == -180) angleCheck = 180;

        string animPrefix = "kick_";
        string directionStr = "south";

        if (angleCheck == 0) directionStr = "east";
        else if (angleCheck == -45) directionStr = "northeast";
        else if (angleCheck == -90) directionStr = "north";
        else if (angleCheck == -135) directionStr = "northwest";
        else if (angleCheck == 180) directionStr = "west";
        else if (angleCheck == 135) directionStr = "southwest";
        else if (angleCheck == 90) directionStr = "south";
        else if (angleCheck == 45) directionStr = "southeast";

        player.animatedSprite2D.Play(animPrefix + directionStr);

        OnAnimationComplete();
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
            .Where(p => p != player && p.TeamID == player.TeamID)
            .ToList();

        playersInView.Sort((p1, p2) =>
            p1.Position.DistanceSquaredTo(player.Position)
            .CompareTo(p2.Position.DistanceSquaredTo(player.Position)));

        return playersInView.Count > 0 ? playersInView[0] : null;
    }
}