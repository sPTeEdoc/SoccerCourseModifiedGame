using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class AIBehaviorField : AIBehavior
{
    public const float PASS_PROBABILITY = 0.05f;
    public const float SHOT_DISTANCE = 150f;
    public const float SHOT_PROBABILITY = 0.3f;
    public const float SPREAD_ASSIST_FACTOR = 0.8f;
    public const float TACKLE_DISTANCE = 15f;
    public const float TACKLE_PROBABILITY = 0.3f;

    public GameManager gameManager;

    public override void PerformAIMovement()
    {
        Vector2 totalSteeringForce = Vector2.Zero;

        if (player.HasBall())
        {
            totalSteeringForce += GetCarrierSteeringForce();
        }
        else if (IsBallCarriedByTeammate())
        {
            totalSteeringForce += GetAssistFormationSteeringForce();
        }
        else
        {
            totalSteeringForce += GetOndutySteeringForce();

            if (totalSteeringForce.LengthSquared() < 1)
            {
                if (IsBallPossessedByOpponent())
                {
                    totalSteeringForce += GetSpawnSteeringForce();
                }
                else if (ball.Carrier == null)
                {
                    totalSteeringForce += GetBallProximitySteeringForce();
                    totalSteeringForce += GetDensityAroundBallSteeringForce();
                }
            }
        }

        totalSteeringForce = totalSteeringForce.LimitLength(1.0f);

        // Calculate if the AI should sprint
        float currentSpeed = player.speed;
        if (ShouldAIEngageSprint())
        {
            currentSpeed *= 1.5f; // Match your human sprint multiplier

            // If the CPU is sprinting with the ball, trigger the micro-kick rule!
            if (player.HasBall())
            {
                TriggerCPUMicroKick(totalSteeringForce, currentSpeed);
            }
        }

        player.Velocity = totalSteeringForce * currentSpeed;
    }

    private bool ShouldAIEngageSprint()
    {
        // Case 1: Chasing a completely loose ball
        if (ball.Carrier == null && player.Position.DistanceTo(ball.Position) > 40f)
        {
            return true;
        }

        // Case 2: Carrier has clear grass ahead towards goal
        if (player.HasBall() && !HasOpponentsNearby())
        {
            return true;
        }

        return false;
    }

    private void TriggerCPUMicroKick(Vector2 direction, float currentSpeed)
    {
        // Every few frames (or handled by a small timer logic inside player character)
        // Push the ball forward into FREEFORM, allowing defenders a chance to intercept
        Vector2 pushVelocity = direction * (currentSpeed * 1.3f);
        ball.Velocity = pushVelocity;
        ball.Carrier = null;
        ball.SwitchState(Ball.State.FREEFORM, BallStateData.Build().SetLockDuration(150));
    }

    public override void PerformAIDecisions()
    {
        if (IsBallPossessedByOpponent() &&
            player.Position.DistanceTo(ball.Position) < TACKLE_DISTANCE &&
            GD.Randf() < TACKLE_PROBABILITY)
        {
            player.SwitchState(PlayerCharacter.State.TACKLING);
        }

        if (ball.Carrier == player)
        {
            Vector2 target = player.targetGoal.GetCenterTargetPosition();
            float shotProbability = SHOT_PROBABILITY;
            gameManager = GetNode<GameManager>("/root/GameManager");

            if (gameManager.playerSetup[0] == player.teamID || gameManager.playerSetup[1] == player.teamID)
            {
                shotProbability /= 10f;
            }
            Random random = new Random();
            double bonus = random.NextDouble();
            float shotPower = player.power * (1.2f + (float)bonus);

            if (player.Position.DistanceTo(target) < SHOT_DISTANCE && GD.Randf() < shotProbability)
            {
                player.FaceTowardsTargetGoal();
                Vector2 shotDirection = player.Position.DirectionTo(player.targetGoal.GetRandomTargetPosition());

                var data = PlayerStateData.Build()
                    .SetShotPower(shotPower)
                    .SetShotDirection(shotDirection);

                player.SwitchState(PlayerCharacter.State.SHOOTING, data);
            }
            else if (GD.Randf() < PASS_PROBABILITY && HasOpponentsNearby() && HasTeammateInView())
            {
                player.SwitchState(PlayerCharacter.State.PASSING);
            }
        }
    }

    public Vector2 GetOndutySteeringForce() =>
        player.weightOnDutySteering * player.Position.DirectionTo(ball.Position);

    public Vector2 GetCarrierSteeringForce()
    {
        Vector2 target = player.targetGoal.GetCenterTargetPosition();
        Vector2 direction = player.Position.DirectionTo(target);
        float weight = GetBicircularWeight(player.Position, target, 100, 0, 150, 1);
        return weight * direction;
    }

    public Vector2 GetAssistFormationSteeringForce()
    {
        Vector2 spawnDifference = ball.Carrier.spawnPosition - player.spawnPosition;
        Vector2 assistDestination = ball.Carrier.Position - spawnDifference * SPREAD_ASSIST_FACTOR;
        Vector2 direction = player.Position.DirectionTo(assistDestination);
        float weight = GetBicircularWeight(player.Position, assistDestination, 30, 0.2f, 60, 1);
        return weight * direction;
    }

    public Vector2 GetBallProximitySteeringForce()
    {
        float weight = GetBicircularWeight(player.Position, ball.Position, 50, 1, 120, 0);
        Vector2 direction = player.Position.DirectionTo(ball.Position);
        return weight * direction;
    }

    public Vector2 GetSpawnSteeringForce()
    {
        float weight = GetBicircularWeight(player.Position, player.spawnPosition, 30, 0, 100, 1);
        Vector2 direction = player.Position.DirectionTo(player.spawnPosition);
        return weight * direction;
    }

    public Vector2 GetDensityAroundBallSteeringForce()
    {
        int nearbyCount = ball.GetProximityTeammatesCount(player.teamID);
        if (nearbyCount == 0)
            return Vector2.Zero;

        float weight = 1f - 1f / nearbyCount;
        Vector2 direction = ball.Position.DirectionTo(player.Position);
        return weight * direction;
    }

    public bool HasTeammateInView()
    {
        var teammates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player && p.teamID == player.teamID);

        return teammates.Any();
    }
}