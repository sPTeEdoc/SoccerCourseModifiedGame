using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class PlayerStatePassing : PlayerState
{
    private const float PASS_FREEZE_DURATION = 0.12f;
    private float freezeTimer = 0f;

    public SoundPlayer soundPlayer;

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");

        freezeTimer = PASS_FREEZE_DURATION;
        player.Velocity = Vector2.Zero;
        player.InputLocked = true;

        soundPlayer.Play(SoundPlayer.Sound.PASS);

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

        // Perform pass directly and do NOT connect AnimationFinished signal to OnAnimationComplete
        PerformPass();
    }

    private void PerformPass()
    {
        PlayerCharacter passTarget = stateData.PassTarget ?? FindTeammateInView();

        if (passTarget == null)
        {
            // â Open-field pass with improved targeting
            float passPowerFactor = 0.85f + (player.GameAttributes.Passing / 100f) * 0.3f;
            float targetDistance = 130f * passPowerFactor; // Increased from 115

            Vector2 destination = ball.Position + player.heading.Normalized() * targetDistance;
            ball.PassTo(destination, receiver: null);
        }
        else
        {
            // â Targeted pass with lead prediction
            float passDistance = player.Position.DistanceTo(passTarget.Position);
            float estimatedTravelTime = passDistance / 200f; // Rough estimate

            Vector2 predictedPos = passTarget.Position +
                passTarget.Velocity * estimatedTravelTime * 0.9f;

            ball.PassTo(predictedPos, receiver: passTarget);
        }
    }

    public override void _Process(double delta)
    {
        freezeTimer -= (float)delta;
        player.Velocity = Vector2.Zero;

        if (freezeTimer <= 0f)
        {
            player.InputLocked = false;
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }

    public override void OnAnimationComplete()
    {
        PlayerCharacter passTarget = stateData.PassTarget ?? FindTeammateInView();

        if (passTarget == null)
        {
            GD.Print($"ball original position: {ball.Position}");
            Vector2 heading = player.heading;
            // if (heading.LengthSquared() < 0.01f)
            //     heading = Vector2.Down;

            heading = heading.Normalized();

            // Open-field pass tuned to match targeted pass pace
            float passPowerFactor = 0.85f + (player.GameAttributes.Passing / 100f) * 0.35f;
            float targetDistance = 115f * passPowerFactor;

            Vector2 destination = ball.Position + heading * targetDistance;
            ball.PassTo(destination, receiver: null);
            GD.Print($"heading: {heading}, player power: {player.GameAttributes.Passing}, destination: {destination}");
        }
        else
        {
            Vector2 predictedPos = passTarget.Position + passTarget.Velocity * 0.8f;
            ball.PassTo(predictedPos, receiver: passTarget);
        }
    }

    private PlayerCharacter FindTeammateInView()
    {
        if (player.heading != Vector2.Zero && teammateDetectionArea != null)
        {
            teammateDetectionArea.Rotation = player.heading.Angle();
        }

        Vector2 passDir = player.heading.Normalized();
        if (passDir == Vector2.Zero) passDir = Vector2.Down;

        var candidates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player &&
                        p.TeamID == player.TeamID &&
                        p.role != PlayerCharacter.Role.GOALIE) // â Don't pass to keeper
            .Select(p =>
            {
                Vector2 toTeammate = p.Position - player.Position;
                float dist = toTeammate.Length();
                Vector2 dir = dist > 0.001f ? toTeammate / dist : Vector2.Zero;
                float dot = passDir.Dot(dir);

                // â Check if passing lane is clear
                bool laneBlocked = IsPassingLaneBlocked(player.Position, p.Position);
                float blockPenalty = laneBlocked ? -500f : 0f;

                return new
                {
                    Player = p,
                    Distance = dist,
                    Dot = dot,
                    Score = (dot * 100f) + (dist * 0.3f) + blockPenalty
                };
            })
            .Where(x => x.Dot > 0.3f) // Must be somewhat forward
            .OrderByDescending(x => x.Score)
            .ToList();

        return candidates.Count > 0 ? candidates[0].Player : null;
    }

    /// <summary>
    /// Checks if opponents block the passing lane.
    /// </summary>
    private bool IsPassingLaneBlocked(Vector2 start, Vector2 end)
    {
        var opponents = player.opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID);

        Vector2 passDirection = (end - start).Normalized();
        float passLength = start.DistanceTo(end);

        foreach (var opponent in opponents)
        {
            Vector2 toOpponent = opponent.Position - start;
            float projection = toOpponent.Dot(passDirection);

            // Only check opponents in the path
            if (projection > 0 && projection < passLength)
            {
                Vector2 closestPoint = start + passDirection * projection;
                float distanceToLane = closestPoint.DistanceTo(opponent.Position);

                if (distanceToLane < 35f) // Within interception range
                {
                    return true;
                }
            }
        }

        return false;
    }
}