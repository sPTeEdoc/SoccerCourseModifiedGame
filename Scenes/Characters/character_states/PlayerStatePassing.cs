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
            // Vector2 heading = player.heading.Normalized();
            // if (heading.LengthSquared() < 0.01f)
            //     heading = player.FacingDirection; // Use last non-zero direction instead of Vector2.Down

            float passPowerFactor = 0.85f + (player.power / 100f) * 0.35f;
            float targetDistance = 115f * passPowerFactor;

            Vector2 destination = ball.Position + player.heading.Normalized() * targetDistance;
            ball.PassTo(destination, receiver: null);
        }
        else
        {
            Vector2 predictedPos = passTarget.Position + passTarget.Velocity * 0.8f;
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
            float passPowerFactor = 0.85f + (player.power / 100f) * 0.35f;
            float targetDistance = 115f * passPowerFactor;

            Vector2 destination = ball.Position + heading * targetDistance;
            ball.PassTo(destination, receiver: null);
            GD.Print($"heading: {heading}, player power: {player.power}, destination: {destination}");
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
            .Where(p => p != player && p.TeamID == player.TeamID)
            .Select(p =>
            {
                Vector2 toTeammate = p.Position - player.Position;
                float dist = toTeammate.Length();
                Vector2 dir = dist > 0.001f ? toTeammate / dist : Vector2.Zero;
                float dot = passDir.Dot(dir); // 1.0 = directly ahead, <0 = behind
                return new { Player = p, Distance = dist, Dot = dot };
            })
            // Ignore teammates behind or sharply to the side of the passer
            .Where(x => x.Dot > 0.2f)
            // Score candidates favoring forward alignment and downfield distance
            .OrderByDescending(x => (x.Dot * 120f) + (x.Distance * 0.4f))
            .ToList();

        return candidates.Count > 0 ? candidates[0].Player : null;
    }
}