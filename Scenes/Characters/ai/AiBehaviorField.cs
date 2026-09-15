using Godot;
using System;
using System.Linq;

/// <summary>
/// Role-specific behavior modifiers.
/// </summary>
public struct RoleBehaviorModifiers
{
    public float OffensivePushMultiplier;
    public float DefensiveDropMultiplier;
    public float ShootingAggression;
    public float PassingCaution;
    public float TacklingAggression;
    public float MaxPushUpLine;
}

[GlobalClass]
public partial class AIBehaviorField : AIBehavior
{
    public const float PASS_PROBABILITY = 0.05f;
    public const float SHOT_DISTANCE = 150f;
    public const float SHOT_PROBABILITY = 0.3f;
    public const float SPREAD_ASSIST_FACTOR = 0.8f;
    public const float TACKLE_DISTANCE = 55f;
    public const float TACKLE_PROBABILITY = 0.3f;
    public const float MARKING_RANGE = 120f;        // Ã¢ NEW - for GetMarkingSteeringForce
    public const float INTERCEPTION_RANGE = 85f;    // Ã¢ NEW - for intelligent positioning
    public const float PRESSING_RANGE = 95f;

    public GameManager gameManager;

    protected float hesitationTimer = 0f;
    protected bool isHesitating = false;
    private float randomMovementTimer = 0f;
    private Vector2 randomDirection = Vector2.Zero;
    private bool justPassedBall = false;
    private PlayerCharacter lastPassRecipient = null;
    private float timeSincePass = 0f;
    private const float DEFENDER_PUSH_UP_LINE = 0.4f;  // 40% up the pitch
    private const float MIDFIELDER_PUSH_UP_LINE = 0.6f; // 60% up the pitch
    private const float FORWARD_FALLBACK_LINE = 0.7f;   // Only drop back to 70%

    public override void PerformAIMovement()
    {
        int awareness = player.GameAttributes.Awareness;
        Vector2 totalSteeringForce = Vector2.Zero;

        // === TIER 6: RANDOM MOVEMENT (0-9) ===
        if (awareness < 10)
        {
            totalSteeringForce = GetRandomMovementForce();
        }
        // === TIER 5: CLUELESS - PURE BALL CHASER (10-19) ===
        else if (awareness < 20)
        {
            if (player.HasBall())
            {
                // Just run at goal
                totalSteeringForce = GetCarrierSteeringForce();
            }
            else
            {
                // Just chase ball
                totalSteeringForce = player.Position.DirectionTo(ball.Position);
            }
        }
        // === TIER 4: BAD REC LEAGUE - BALL CHASER WITH PAUSES (20-39) ===
        else if (awareness < 40)
        {
            // Update hesitation state
            UpdateHesitation(0.2f); // Pass delta time (approximation)

            if (isHesitating)
            {
                // Pause when "sort of" in position
                player.Velocity = Vector2.Zero;
                return;
            }

            if (player.HasBall())
            {
                totalSteeringForce = GetCarrierSteeringForce();
            }
            else if (IsBallCarriedByTeammate())
            {
                // Vague sense of position, but mostly chase
                float distanceToSpawn = player.Position.DistanceTo(player.spawnPosition);

                if (distanceToSpawn < 100f)
                {
                    // "Sort of" where they should be - pause more often
                    if (GD.Randf() < 0.3f) // 30% chance to pause
                    {
                        isHesitating = true;
                        hesitationTimer = GD.Randf() * 2f + 1f; // 1-3 seconds
                        player.Velocity = Vector2.Zero;
                        return;
                    }
                }

                // Otherwise, chase ball
                totalSteeringForce = player.Position.DirectionTo(ball.Position) * 0.7f;
            }
            else
            {
                // Loose ball - chase it
                totalSteeringForce = player.Position.DirectionTo(ball.Position);
            }
        }
        // === TIER 3: REC LEAGUE - BASIC UNDERSTANDING (40-49) ===
        else if (awareness < 50)
        {
            // Occasional hesitation
            UpdateHesitation(0.2f);

            if (isHesitating)
            {
                player.Velocity = Vector2.Zero;
                return;
            }

            if (player.HasBall())
            {
                totalSteeringForce = GetCarrierSteeringForce();
            }
            else if (IsBallCarriedByTeammate())
            {
                // Move toward general position, but slowly
                totalSteeringForce = GetAssistFormationSteeringForce() * 0.6f;
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
        }
        // === TIER 2: ACADEMY - KNOWS FUNDAMENTALS, HESITANT (50-69) ===
        else if (awareness < 70)
        {
            // Less frequent hesitation
            UpdateHesitation(0.2f);

            if (isHesitating)
            {
                player.Velocity = Vector2.Zero;
                return;
            }

            // Use standard logic but without advanced features
            if (player.HasBall())
            {
                totalSteeringForce += GetCarrierSteeringForce();
            }
            else if (IsBallCarriedByTeammate())
            {
                totalSteeringForce += GetAssistFormationSteeringForce();
                // NO intelligent runs (awareness < 70)
            }
            else
            {
                totalSteeringForce += GetOndutySteeringForce();

                if (totalSteeringForce.LengthSquared() < 1)
                {
                    if (IsBallPossessedByOpponent())
                    {
                        if (player.GameAttributes.Defense >= 50)
                        {
                            totalSteeringForce += GetMarkingSteeringForce();
                        }
                        else
                        {
                            totalSteeringForce += GetSpawnSteeringForce();
                        }
                    }
                    else if (ball.Carrier == null)
                    {
                        totalSteeringForce += GetBallProximitySteeringForce();
                        totalSteeringForce += GetDensityAroundBallSteeringForce();
                    }
                }
            }
        }
        // === TIER 1: PROFESSIONAL & ELITE (70+) ===
        else
        {
            var roleModifiers = GetRoleModifiers();

            if (player.HasBall())
            {
                totalSteeringForce += GetCarrierSteeringForce();
            }
            else if (IsBallCarriedByTeammate())
            {
                totalSteeringForce += GetAssistFormationSteeringForce() * roleModifiers.OffensivePushMultiplier;
                totalSteeringForce += GetIntelligentRunSteeringForce() * 0.6f;

                if (player.role == PlayerCharacter.OnFieldPositions.DEFENSE && ShouldAttemptOverlap())
                {
                    Vector2 overlapDestination = ball.Carrier.Position + ball.Carrier.heading * 120f;
                    Vector2 overlapDirection = player.Position.DirectionTo(overlapDestination);
                    totalSteeringForce += overlapDirection * 0.8f;
                }

                totalSteeringForce += GetRolePositioningForce();
            }
            else
            {
                totalSteeringForce += GetOndutySteeringForce();

                if (totalSteeringForce.LengthSquared() < 1)
                {
                    if (IsBallPossessedByOpponent())
                    {
                        float distanceToBall = player.Position.DistanceTo(ball.Position);

                        // Ã¢ LAYERED DEFENSIVE LOGIC

                        // 1. Jockey if in middle distance
                        Vector2 jockeyForce = GetJockeyingForce();
                        if (jockeyForce.LengthSquared() > 0.01f)
                        {
                            totalSteeringForce += jockeyForce * 0.9f;
                        }

                        // 2. Press if close enough and high awareness
                        if (player.GameAttributes.Awareness >= 60 && distanceToBall < PRESSING_RANGE)
                        {
                            Vector2 pressingForce = GetPressingSteeringForce();
                            if (pressingForce.LengthSquared() > 0.1f)
                            {
                                totalSteeringForce += pressingForce * 1.0f;  // Ã¢ Was 0.7f
                            }
                        }

                        // 3. Mark nearby opponents
                        if (player.GameAttributes.Defense >= 50)
                        {
                            totalSteeringForce += GetMarkingSteeringForce() * 0.8f;
                        }

                        // 4. Drop back to position if not engaged
                        if (totalSteeringForce.LengthSquared() < 0.5f)
                        {
                            totalSteeringForce += GetSpawnSteeringForce() * roleModifiers.DefensiveDropMultiplier;
                        }
                    }
                    else if (ball.Carrier == null)
                    {
                        totalSteeringForce += GetBallProximitySteeringForce();
                        totalSteeringForce += GetDensityAroundBallSteeringForce();
                    }
                }
            }
        }

        // Apply separation for all tiers (avoid clustering)
        float separationMultiplier = 1.2f;

        if (IsBallPossessedByOpponent())
        {
            // â­ PRIORITY 0: Block shots (OVERRIDE EVERYTHING)
            Vector2 shotBlockForce = GetShotBlockingForce();
            if (shotBlockForce.LengthSquared() > 0.1f)
            {
                totalSteeringForce += shotBlockForce * 3.0f; // MAXIMUM urgency
                // Don't return - let other forces contribute slightly for smoothness
            }

            float distanceToBall = player.Position.DistanceTo(ball.Position);

            // â PRIORITY 1: Recovery run if beaten
            Vector2 recoveryForce = GetRecoveryRunForce();
            if (recoveryForce.LengthSquared() > 0.1f)
            {
                totalSteeringForce += recoveryForce * 1.8f; // OVERRIDE everything
            }
            else
            {
                // â PRIORITY 2: Jockey if in middle distance
                Vector2 jockeyForce = GetJockeyingForce();
                if (jockeyForce.LengthSquared() > 0.01f)
                {
                    totalSteeringForce += jockeyForce * 0.9f;
                }

                // â PRIORITY 3: Press if close enough
                if (player.GameAttributes.Awareness >= 60 && distanceToBall < PRESSING_RANGE)
                {
                    Vector2 pressingForce = GetPressingSteeringForce();
                    if (pressingForce.LengthSquared() > 0.1f)
                    {
                        totalSteeringForce += pressingForce * 1.0f;
                    }
                }

                // â PRIORITY 4: Mark nearby opponents
                if (player.GameAttributes.Defense >= 50)
                {
                    totalSteeringForce += GetMarkingSteeringForce() * 0.8f;
                }

                // â PRIORITY 5: Drop back to position if not engaged
                if (totalSteeringForce.LengthSquared() < 0.5f)
                {
                    var roleModifiers = GetRoleModifiers();
                    totalSteeringForce += GetSpawnSteeringForce() * roleModifiers.DefensiveDropMultiplier;
                }
            }
        }
        else if (ball.Carrier == null)
        {
            // â PRIORITY 1: Intercept passes
            totalSteeringForce += GetPassInterceptionForce() * 1.6f;

            // â PRIORITY 2: Block shots
            totalSteeringForce += GetShotBlockingForce() * 2.0f;

            // â PRIORITY 3: Chase loose ball
            totalSteeringForce += GetBallProximitySteeringForce();
            totalSteeringForce += GetDensityAroundBallSteeringForce();
        }

        totalSteeringForce += GetSeparationSteeringForce() * separationMultiplier;
        totalSteeringForce = totalSteeringForce.LimitLength(1.0f);
        player.Velocity = totalSteeringForce * player.GameAttributes.Speed;
    }

    private void UpdateHesitation(float delta)
    {
        int awareness = player.GameAttributes.Awareness;

        // No hesitation for competent+ players
        if (awareness >= 70)
            return;

        if (isHesitating)
        {
            hesitationTimer -= delta;
            if (hesitationTimer <= 0f)
            {
                isHesitating = false;
            }
        }
        else
        {
            // Hesitation chance inversely proportional to awareness
            // 20 awareness = ~6.7% chance per tick
            // 50 awareness = ~3.3% chance per tick
            // 69 awareness = ~1.7% chance per tick
            float hesitationChance = (70f - awareness) / 1000f;

            if (GD.Randf() < hesitationChance)
            {
                isHesitating = true;
                // Lower awareness = longer pauses
                float pauseDuration = ((70f - awareness) / 50f) + 0.5f;
                hesitationTimer = GD.Randf() * pauseDuration + 0.5f; // 0.5-2.5 seconds
            }
        }
    }

    /// <summary>
    /// Random movement for awareness 0-9 players.
    /// </summary>
    private Vector2 GetRandomMovementForce()
    {
        randomMovementTimer -= 0.2f; // Approximate delta

        if (randomMovementTimer <= 0f)
        {
            // Pick new random direction every 1-3 seconds
            randomDirection = new Vector2(
                GD.Randf() * 2f - 1f,
                GD.Randf() * 2f - 1f
            ).Normalized();

            randomMovementTimer = GD.Randf() * 2f + 1f;
        }

        return randomDirection;
    }

    public Vector2 GetMarkingSteeringForce()
    {
        if (!IsBallPossessedByOpponent())
            return Vector2.Zero;

        var nearbyOpponents = opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID)
            .OrderBy(p => p.Position.DistanceTo(ball.Position));

        var dangerousOpponent = nearbyOpponents.FirstOrDefault();

        if (dangerousOpponent == null)
            return Vector2.Zero;

        float distanceToOpponent = player.Position.DistanceTo(dangerousOpponent.Position);

        // Ã¢ Only mark if within marking range
        if (distanceToOpponent > MARKING_RANGE)
            return Vector2.Zero;

        // Ã¢ Position between opponent and own goal (classic man-marking)
        Vector2 toOwnGoal = player.Position.DirectionTo(player.ownGoal.Position);

        // Ã¢ CRITICAL: Maintain 35-50px cushion based on opponent speed
        float cushionDistance = 35f + (dangerousOpponent.GameAttributes.Speed / 100f * 15f);
        Vector2 markingPosition = dangerousOpponent.Position + toOwnGoal.Normalized() * cushionDistance;

        float defenseModifier = player.GameAttributes.Defense / 100f;
        Vector2 direction = player.Position.DirectionTo(markingPosition);

        // Ã¢ Weight decreases with distance - urgent when close
        float weight = 1f - (distanceToOpponent / MARKING_RANGE);
        weight = Mathf.Pow(weight, 1.5f);

        return direction * weight * defenseModifier * 0.85f;
    }

    public Vector2 GetPressingSteeringForce()
    {
        if (!IsBallPossessedByOpponent() || ball.Carrier == null)
            return Vector2.Zero;

        // Only high-awareness players press effectively
        if (player.GameAttributes.Awareness < 60)
            return Vector2.Zero;

        float distanceToBall = player.Position.DistanceTo(ball.Position);

        // Ã¢ Don't press if too far - maintain shape instead
        if (distanceToBall > PRESSING_RANGE)
            return Vector2.Zero;

        // Ã¢ CORE FIX: Defenders intercept path, don't just chase carrier
        Vector2 carrierVelocity = ball.Carrier.Velocity;
        Vector2 interceptionPoint;

        if (carrierVelocity.LengthSquared() > 1f)
        {
            // Predict where carrier will be in 0.4 seconds
            float predictionTime = 0.4f;
            interceptionPoint = ball.Carrier.Position + carrierVelocity * predictionTime;
        }
        else
        {
            // Carrier is stationary - press directly
            interceptionPoint = ball.Carrier.Position;
        }

        Vector2 direction = player.Position.DirectionTo(interceptionPoint);
        float distanceToIntercept = player.Position.DistanceTo(interceptionPoint);

        // Ã¢ Only commit to press if close enough
        if (distanceToIntercept > PRESSING_RANGE)
            return Vector2.Zero;

        // Ã¢ Exponential urgency curve - slow approach, fast close-in
        float urgency = 1f - (distanceToIntercept / PRESSING_RANGE);
        urgency = Mathf.Pow(urgency, 2.2f);  // Squared curve for gradual engagement

        float pressIntensity = player.GameAttributes.Awareness / 100f;
        var roleModifiers = GetRoleModifiers();

        // Ã¢ Role-based aggression
        float roleMultiplier = player.role == PlayerCharacter.OnFieldPositions.DEFENSE ? 1.3f : 0.9f;

        return direction * urgency * pressIntensity * roleModifiers.TacklingAggression * roleMultiplier;
    }

    public bool ShouldAttemptOverlap()
    {
        // Only defenders
        if (player.role != PlayerCharacter.OnFieldPositions.DEFENSE)
            return false;

        // Need high awareness and offense
        if (player.GameAttributes.Awareness < 75 ||
            player.GameAttributes.Offense < 70)
            return false;

        // Team must be in attacking third
        float pitchLength = Mathf.Abs(player.targetGoal.Position.Y - player.ownGoal.Position.Y);
        float attackingThirdLine = player.ownGoal.Position.Y +
            (player.targetGoal.Position.Y - player.ownGoal.Position.Y) * 0.66f;

        if ((ball.Position.Y - attackingThirdLine) * Mathf.Sign(player.targetGoal.Position.Y - player.ownGoal.Position.Y) < 0)
            return false;

        // Ball carrier must be on wing (not central)
        if (Mathf.Abs(ball.Carrier.Position.X - player.Position.X) < 100f)
            return false;

        // Need defensive cover
        // (You'd need to implement CountDefendersInDefensiveThird())

        // Small probability
        return GD.Randf() < 0.15f;
    }

    public float GetUrgencyModifier()
    {
        gameManager = GetNode<GameManager>("/root/GameManager");

        int myScore = gameManager.currentMatch.GoalsHome;
        int opponentScore = gameManager.currentMatch.GoalsAway;

        if (player.TeamID == gameManager.currentMatch.AwayTeam)
        {
            opponentScore = gameManager.currentMatch.GoalsHome;
            myScore = gameManager.currentMatch.GoalsAway;
        }

        // Ã¢ FIX: Correct time remaining calculation
        // Assuming TimeElapsed is in seconds and IN_GAME_MINUTES_PER_HALF is in minutes
        float halfDurationSeconds = gameManager.IN_GAME_MINUTES_PER_HALF * 60f;
        float timeRemaining = halfDurationSeconds - gameManager.TimeElapsed;

        int scoreDifference = myScore - opponentScore;

        // Losing in final 10 minutes = more aggressive
        if (scoreDifference < 0 && timeRemaining < 600f) // 10 minutes
        {
            // The more desperate, the more aggressive
            float desperationFactor = Mathf.Clamp((600f - timeRemaining) / 600f, 0f, 1f);
            return 1.0f + (0.5f * desperationFactor); // 1.0 to 1.5
        }

        // Winning in final 5 minutes = more conservative
        if (scoreDifference > 0 && timeRemaining < 300f) // 5 minutes
        {
            // The closer to end, the more conservative
            float conservationFactor = Mathf.Clamp((300f - timeRemaining) / 300f, 0f, 1f);
            return 1.0f - (0.3f * conservationFactor); // 1.0 to 0.7
        }

        return 1.0f; // Normal behavior
    }

    public Vector2 GetIntelligentRunSteeringForce()
    {
        // Only make runs occasionally
        if (GD.Randf() > 0.1f) // 10% chance per AI tick
            return Vector2.Zero;

        // Find space ahead of ball carrier
        Vector2 ballCarrierDirection = ball.Carrier.heading;
        Vector2 potentialRunDestination = ball.Carrier.Position +
            ballCarrierDirection * 150f;

        // Check if space is open
        var opponentsNearTarget = opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID &&
                        p.Position.DistanceTo(potentialRunDestination) < 50f);

        if (!opponentsNearTarget.Any())
        {
            Vector2 direction = player.Position.DirectionTo(potentialRunDestination);
            return direction * 0.8f;
        }

        return Vector2.Zero;
    }

    public Vector2 GetGiveAndGoSteeringForce()
    {
        if (!justPassedBall || lastPassRecipient == null)
            return Vector2.Zero;

        if (timeSincePass < 2f && lastPassRecipient.HasBall())
        {
            Vector2 runDestination = lastPassRecipient.Position + player.heading * 80f;
            return player.Position.DirectionTo(runDestination) * 0.9f;
        }

        justPassedBall = false;
        return Vector2.Zero;
    }

    public bool IsOutOfPosition(float threshold = 100f)
    {
        float distanceFromSpawn = player.Position.DistanceTo(player.spawnPosition);
        return distanceFromSpawn > threshold;
    }

    // Stronger pull back when too far from position
    public Vector2 GetFormationMaintenanceForce()
    {
        if (!IsOutOfPosition())
            return Vector2.Zero;

        float distance = player.Position.DistanceTo(player.spawnPosition);
        float urgency = Mathf.Clamp((distance - 100f) / 200f, 0f, 1f);

        return player.Position.DirectionTo(player.spawnPosition) * urgency;
    }

    public bool IsCounterAttackOpportunity()
    {
        if (ball.Carrier == null || ball.Carrier.TeamID != player.TeamID)
            return false;

        // Just won possession in own half?
        float ownHalfLine = (player.ownGoal.Position.Y + player.targetGoal.Position.Y) / 2f;
        bool inOwnHalf = (ball.Position.Y - ownHalfLine) *
            Mathf.Sign(player.ownGoal.Position.Y - player.targetGoal.Position.Y) < 0;

        if (!inOwnHalf)
            return false;

        // Opponents out of position?
        var opponentsInAttackingHalf = opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID &&
                        (p.Position.Y - ownHalfLine) *
                        Mathf.Sign(player.targetGoal.Position.Y - ownHalfLine) > 0);

        return opponentsInAttackingHalf.Count() >= 5; // Most opponents caught upfield
    }

    public override void PerformAIDecisions()
    {
        int awareness = player.GameAttributes.Awareness;
        float awarenessModifier = awareness / 100f;

        // === TIER 6: RANDOM (0-9) - No decisions, just random movement ===
        if (awareness < 10)
        {
            return; // Movement handles everything
        }

        // === TIER 5: CLUELESS (10-19) - Only shoot wildly ===
        if (awareness < 20)
        {
            if (ball.Carrier == player)
            {
                // Shoot from anywhere, any time
                if (GD.Randf() < 0.3f) // 30% chance every tick
                {
                    player.FaceTowardsTargetGoal();
                    Vector2 shotDirection = player.Position.DirectionTo(
                        player.targetGoal.GetRandomTargetPosition());

                    var data = PlayerStateData.Build()
                        .SetShotPower(player.GameAttributes.Shooting * 0.3f) // Weak, wild shot
                        .SetShotDirection(shotDirection);

                    player.SwitchState(PlayerCharacter.State.SHOOTING, data);
                }
            }
            return; // No other decisions
        }

        // === TIER 4: BAD REC LEAGUE (20-39) - Basic shoot/pass ===
        if (awareness < 40)
        {
            if (ball.Carrier == player)
            {
                // Shoot often, even from bad positions
                if (GD.Randf() < 0.2f)
                {
                    player.FaceTowardsTargetGoal();
                    Vector2 shotDirection = player.Position.DirectionTo(
                        player.targetGoal.GetRandomTargetPosition());

                    var data = PlayerStateData.Build()
                        .SetShotPower(player.GameAttributes.Shooting * 0.6f)
                        .SetShotDirection(shotDirection);

                    player.SwitchState(PlayerCharacter.State.SHOOTING, data);
                    return;
                }

                // Rarely pass
                if (GD.Randf() < 0.02f && HasTeammateInView())
                {
                    var nearestTeammate = FindNearestTeammate();
                    if (nearestTeammate != null)
                    {
                        player.SwitchState(PlayerCharacter.State.PASSING,
                            PlayerStateData.Build().SetPassTarget(nearestTeammate));
                    }
                }
            }

            // Tackle randomly when close
            if (IsBallPossessedByOpponent() &&
                player.Position.DistanceTo(ball.Position) < TACKLE_DISTANCE &&
                GD.Randf() < 0.15f) // Low tackle awareness
            {
                player.SwitchState(PlayerCharacter.State.TACKLING);
            }

            return;
        }

        // === TIER 3: REC LEAGUE (40-49) - Understands basics ===
        if (awareness < 50)
        {
            if (ball.Carrier == player)
            {
                Vector2 target = player.targetGoal.GetCenterTargetPosition();
                float distanceToGoal = player.Position.DistanceTo(target);

                // Shoot if somewhat close
                if (distanceToGoal < SHOT_DISTANCE * 1.5f && GD.Randf() < 0.15f)
                {
                    player.FaceTowardsTargetGoal();
                    Vector2 shotDirection = player.Position.DirectionTo(
                        player.targetGoal.GetRandomTargetPosition());

                    var data = PlayerStateData.Build()
                        .SetShotPower(player.GameAttributes.Shooting * 0.8f)
                        .SetShotDirection(shotDirection);

                    player.SwitchState(PlayerCharacter.State.SHOOTING, data);
                    return;
                }

                // Pass to nearest teammate if under pressure
                if (HasOpponentsNearby() && GD.Randf() < 0.08f && HasTeammateInView())
                {
                    var nearestTeammate = FindNearestTeammate();
                    if (nearestTeammate != null)
                    {
                        player.SwitchState(PlayerCharacter.State.PASSING,
                            PlayerStateData.Build().SetPassTarget(nearestTeammate));
                    }
                }
            }

            // Tackle when close
            if (IsBallPossessedByOpponent() &&
                player.Position.DistanceTo(ball.Position) < TACKLE_DISTANCE &&
                GD.Randf() < (TACKLE_PROBABILITY * 0.6f))
            {
                player.SwitchState(PlayerCharacter.State.TACKLING);
            }

            return;
        }

        // === TIERS 2 & 1: ACADEMY+ (50+) - Full decision tree ===
        // (Use your existing PerformAIDecisions logic from line ~220 onwards)

        gameManager = GetNode<GameManager>("/root/GameManager");

        // === DEFENSIVE ACTIONS ===
        if (IsBallPossessedByOpponent())
        {
            var roleModifiers = GetRoleModifiers(); // Ã¢ ADD THIS
            float tackleProbability = TACKLE_PROBABILITY *
                awarenessModifier *
                roleModifiers.TacklingAggression; // Ã¢ ADD THIS

            if (player.Position.DistanceTo(ball.Position) < TACKLE_DISTANCE &&
                GD.Randf() < tackleProbability)
            {
                player.SwitchState(PlayerCharacter.State.TACKLING);
                return;
            }
        }

        // === OFFENSIVE ACTIONS ===
        if (ball.Carrier == player)
        {
            Vector2 target = player.targetGoal.GetCenterTargetPosition();
            float distanceToGoal = player.Position.DistanceTo(target);

            // 1. THROUGH BALL (High awareness only - 75+)
            if (awareness >= 75)
            {
                var throughBallTarget = FindTeammateForThroughBall();
                if (throughBallTarget != null)
                {
                    float urgencyModifier2 = GetUrgencyModifier();
                    float throughBallChance = 0.2f * awarenessModifier * urgencyModifier2;
                    if (GD.Randf() < throughBallChance)
                    {
                        player.SwitchState(PlayerCharacter.State.PASSING,
                            PlayerStateData.Build().SetPassTarget(throughBallTarget));
                        return;
                    }
                }
            }

            // 2. SHOOTING
            var roleModifiers = GetRoleModifiers(); // Ã¢ ADD THIS
            float shootingModifier = player.GameAttributes.Shooting / 100f;
            float urgencyModifier = GetUrgencyModifier();
            float shotProbability = SHOT_PROBABILITY *
                awarenessModifier *
                shootingModifier *
                urgencyModifier *
                roleModifiers.ShootingAggression;

            // Reduce for player-controlled teammates
            if (gameManager.playerSetup[0] == player.TeamID ||
                gameManager.playerSetup[1] == player.TeamID)
            {
                shotProbability *= 0.1f;
            }

            if (distanceToGoal < SHOT_DISTANCE && GD.Randf() < shotProbability)
            {
                player.FaceTowardsTargetGoal();
                Vector2 shotDirection = player.Position.DirectionTo(
                    player.targetGoal.GetRandomTargetPosition());

                var data = PlayerStateData.Build()
                    .SetShotPower(player.GameAttributes.Shooting)
                    .SetShotDirection(shotDirection);

                player.SwitchState(PlayerCharacter.State.SHOOTING, data);
                return;
            }

            // 3. PASSING
            float passProbability = PASS_PROBABILITY *
    awarenessModifier *
    urgencyModifier *
    roleModifiers.PassingCaution;

            // â REDUCED BASE - AI passes less often (more dribbling)
            passProbability *= 0.6f; // Was 1.0

            // Higher urgency under pressure
            if (HasOpponentsNearby())
            {
                passProbability *= 3f; // Was 2f - pass more when pressured
            }

            // â DON'T PASS if no good forward options
            if (GD.Randf() < passProbability && HasTeammateInView())
            {
                PlayerCharacter passTarget;

                // High awareness finds better targets
                if (awareness >= 70)
                {
                    passTarget = FindBestPassTarget();

                    // â VALIDATE - only pass if score is positive
                    if (passTarget != null)
                    {
                        float passScore = CalculatePassScore(passTarget);
                        if (passScore < 100f) // Threshold for "good pass"
                        {
                            passTarget = null; // Cancel pass - not worth it
                        }
                    }
                }
                else
                {
                    passTarget = FindNearestTeammate();
                }

                if (passTarget != null)
                {
                    player.SwitchState(PlayerCharacter.State.PASSING,
                        PlayerStateData.Build().SetPassTarget(passTarget));
                    return;
                }
            }

            // 4. LONG PASS (Professional+ only - 70+)
            if (awareness >= 70 && distanceToGoal > SHOT_DISTANCE * 2)
            {
                var distantTeammate = FindFarthestOpenTeammate(200f);
                if (distantTeammate != null)
                {
                    float longPassChance = 0.1f * awarenessModifier * urgencyModifier;
                    if (GD.Randf() < longPassChance)
                    {
                        player.SwitchState(PlayerCharacter.State.PASSING,
                            PlayerStateData.Build().SetPassTarget(distantTeammate));
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds the best pass target based on multiple factors.
    /// Used by high-awareness players (70+).
    /// </summary>
    private PlayerCharacter FindBestPassTarget()
    {
        var teammates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player &&
                        p.TeamID == player.TeamID &&
                        p.role != PlayerCharacter.OnFieldPositions.GOALIE); // Don't pass to keeper

        if (!teammates.Any())
            return null;

        PlayerCharacter bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var teammate in teammates)
        {
            float score = CalculatePassScore(teammate);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = teammate;
            }
        }

        return bestTarget;
    }

    /// <summary>
    /// Calculates how good a passing option a teammate is.
    /// Higher score = better target.
    /// </summary>
    private float CalculatePassScore(PlayerCharacter teammate)
    {
        float score = 0f;

        // === FACTOR 1: Proximity to Goal (Closer = Better) ===
        float distToGoal = teammate.Position.DistanceTo(player.targetGoal.Position);
        float maxDist = 1000f;
        score += (maxDist - distToGoal) * 0.6f; // Increased weight

        // === FACTOR 2: Open Space (No Defenders Nearby) ===
        var defendersNearTeammate = player.opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID &&
                        p.Position.DistanceTo(teammate.Position) < 50f);

        if (!defendersNearTeammate.Any())
        {
            score += 500f; // Big bonus for being unmarked
        }
        else
        {
            score -= defendersNearTeammate.Count() * 150f; // Penalty for each nearby defender
        }

        // === FACTOR 3: Movement Toward Goal ===
        if (teammate.Velocity.Length() > 1f)
        {
            Vector2 toGoal = teammate.Position.DirectionTo(player.targetGoal.Position);
            float movementDot = teammate.Velocity.Normalized().Dot(toGoal);

            if (movementDot > 0.5f) // Running toward goal
            {
                score += movementDot * 400f; // Increased bonus
            }
        }

        // === FACTOR 4: Passing Distance (Prefer Medium Range) ===
        float passDistance = player.Position.DistanceTo(teammate.Position);

        if (passDistance > 280f) // Was 250f
        {
            score -= 400f; // Long pass penalty
        }
        else if (passDistance < 40f) // Was 50f
        {
            score -= 150f; // Too close = wasted pass
        }
        else if (passDistance >= 80f && passDistance <= 150f)
        {
            score += 200f; // â SWEET SPOT - ideal pass range
        }

        // === FACTOR 5: Clear Passing Lane ===
        if (!IsPassingLaneClear(player.Position, teammate.Position))
        {
            score -= 600f; // Huge penalty if pass likely intercepted
        }

        // === FACTOR 6: FORWARD PASS MASSIVE BONUS ===
        Vector2 toGoal2 = player.Position.DirectionTo(player.targetGoal.Position);
        Vector2 toTeammate = player.Position.DirectionTo(teammate.Position);
        float forwardDot = toTeammate.Dot(toGoal2);

        if (forwardDot > 0.8f) // Pass toward goal
        {
            score += 350f; // â INCREASED - strongly prefer forward
        }
        else if (forwardDot > 0.4f) // Diagonal forward
        {
            score += 150f; // Still good
        }
        else if (forwardDot < -0.2f) // Backward pass
        {
            score -= 300f; // â HARSH PENALTY - avoid unless necessary
        }

        return score;
    }

    /// <summary>
    /// Checks if there's a clear passing lane between two positions.
    /// </summary>
    private bool IsPassingLaneClear(Vector2 start, Vector2 end)
    {
        var opponents = opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID);

        foreach (var opponent in opponents)
        {
            // Calculate if opponent is in the passing lane
            Vector2 toOpponent = opponent.Position - start;
            Vector2 passDirection = (end - start).Normalized();
            float projection = toOpponent.Dot(passDirection);

            // Only check opponents between passer and receiver
            if (projection > 0 && projection < start.DistanceTo(end))
            {
                Vector2 closestPoint = start + passDirection * projection;
                float distanceToLane = closestPoint.DistanceTo(opponent.Position);

                if (distanceToLane < 40f) // Within interception range
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Finds the nearest teammate.
    /// Used by low-awareness players (<70) for simple passing.
    /// </summary>
    private PlayerCharacter FindNearestTeammate()
    {
        var teammates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player &&
                        p.TeamID == player.TeamID &&
                        p.role != PlayerCharacter.OnFieldPositions.GOALIE);

        if (!teammates.Any())
            return null;

        return teammates
            .OrderBy(p => p.Position.DistanceTo(player.Position))
            .FirstOrDefault();
    }

    private PlayerCharacter FindFarthestOpenTeammate(float minDistance)
    {
        var teammates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player &&
                        p.TeamID == player.TeamID &&
                        p.Position.DistanceTo(player.Position) > minDistance)
            .OrderByDescending(p => p.Position.DistanceTo(player.Position));

        return teammates.FirstOrDefault();
    }

    private PlayerCharacter FindTeammateForThroughBall()
    {
        if (player.GameAttributes.Awareness < 75)
            return null;

        var teammates = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player && p.TeamID == player.TeamID);

        foreach (var teammate in teammates)
        {
            // Check if teammate is running toward goal
            Vector2 toGoal = player.Position.DirectionTo(player.targetGoal.Position);
            Vector2 teammateDirection = teammate.Velocity.Normalized();

            // Dot product > 0.7 means running same direction
            if (teammateDirection.Dot(toGoal) > 0.7f)
            {
                // Check if space exists behind defenders
                Vector2 throughBallTarget = teammate.Position + teammateDirection * 100f;

                if (!IsDefenderBetween(player.Position, throughBallTarget))
                {
                    return teammate;
                }
            }
        }

        return null;
    }

    private bool IsDefenderBetween(Vector2 start, Vector2 end)
    {
        var opponents = opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p.TeamID != player.TeamID);

        foreach (var opponent in opponents)
        {
            // Check if opponent is in the path
            Vector2 toOpponent = opponent.Position - start;
            Vector2 passDirection = (end - start).Normalized();
            float projection = toOpponent.Dot(passDirection);

            if (projection > 0 && projection < start.DistanceTo(end))
            {
                Vector2 closestPoint = start + passDirection * projection;
                if (closestPoint.DistanceTo(opponent.Position) < 30f)
                {
                    return true; // Defender in the way
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets role-specific behavior modifiers.
    /// </summary>
    private RoleBehaviorModifiers GetRoleModifiers()
    {
        switch (player.role)
        {
            case PlayerCharacter.OnFieldPositions.DEFENSE:
                return new RoleBehaviorModifiers
                {
                    OffensivePushMultiplier = 0.6f,  // Defenders push forward less
                    DefensiveDropMultiplier = 1.4f,  // Drop back faster
                    ShootingAggression = 0.3f,       // Rarely shoot
                    PassingCaution = 1.3f,           // More conservative passes
                    TacklingAggression = 1.5f,       // Tackle more aggressively
                    MaxPushUpLine = DEFENDER_PUSH_UP_LINE
                };

            case PlayerCharacter.OnFieldPositions.MIDFIELD:
                return new RoleBehaviorModifiers
                {
                    OffensivePushMultiplier = 1.0f,  // Balanced
                    DefensiveDropMultiplier = 1.0f,  // Balanced
                    ShootingAggression = 0.8f,       // Moderate shooting
                    PassingCaution = 1.0f,           // Balanced passing
                    TacklingAggression = 1.0f,       // Balanced tackling
                    MaxPushUpLine = MIDFIELDER_PUSH_UP_LINE
                };

            case PlayerCharacter.OnFieldPositions.FORWARD:
                return new RoleBehaviorModifiers
                {
                    OffensivePushMultiplier = 1.5f,  // Push forward aggressively
                    DefensiveDropMultiplier = 0.5f,  // Reluctant to drop back
                    ShootingAggression = 1.8f,       // Shoot frequently
                    PassingCaution = 0.8f,           // More risky passes
                    TacklingAggression = 0.6f,       // Less tackling
                    MaxPushUpLine = FORWARD_FALLBACK_LINE
                };

            default: // GOALIE (handled separately)
                return new RoleBehaviorModifiers();
        }
    }

    /// <summary>
    /// Checks if player is in their appropriate zone based on role.
    /// </summary>
    private bool IsInCorrectZone()
    {
        float pitchLength = Mathf.Abs(player.targetGoal.Position.Y - player.ownGoal.Position.Y);
        float playerPositionNormalized = (player.Position.Y - player.ownGoal.Position.Y) / pitchLength;

        // Normalize to 0-1 (0 = own goal, 1 = target goal)
        if (player.ownGoal.Position.Y > player.targetGoal.Position.Y)
        {
            playerPositionNormalized = 1f - playerPositionNormalized;
        }

        var modifiers = GetRoleModifiers();

        // Check if player is beyond their max push-up line
        if (IsBallCarriedByTeammate())
        {
            return playerPositionNormalized <= modifiers.MaxPushUpLine;
        }

        return true; // Allow full movement when defending or player has ball
    }

    /// <summary>
    /// Gets role-specific positioning adjustment.
    /// Prevents defenders from pushing too far forward, etc.
    /// </summary>
    private Vector2 GetRolePositioningForce()
    {
        if (IsInCorrectZone())
            return Vector2.Zero;

        // Player is too far forward for their role - pull them back
        float pullStrength = 0.5f;
        return player.Position.DirectionTo(player.spawnPosition) * pullStrength;
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

        // âœ… Apply Offense rating
        float offenseModifier = player.GameAttributes.Offense / 100f;
        float weight = GetBicircularWeight(player.Position, assistDestination, 30, 0.2f, 60, 1);

        return weight * direction * offenseModifier;
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

        // âœ… Apply Defense rating
        float defenseModifier = player.GameAttributes.Defense / 100f;
        return weight * direction * defenseModifier;
    }

    public Vector2 GetDensityAroundBallSteeringForce()
    {
        int nearbyCount = ball.GetProximityTeammatesCount(player.TeamID);
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
            .Where(p => p != player && p.TeamID == player.TeamID);

        return teammates.Any();
    }

    public Vector2 GetSeparationSteeringForce(float neighborRadius = 25f)
    {
        Vector2 separation = Vector2.Zero;
        int count = 0;

        // Grab nearby players via area detection or scene tree
        var nearbyPlayers = teammateDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Where(p => p != player && p.Position.DistanceTo(player.Position) < neighborRadius);

        foreach (var other in nearbyPlayers)
        {
            Vector2 diff = player.Position - other.Position;
            float distance = diff.Length();

            if (distance > 0)
            {
                // Inverse weighting: closer neighbors exert stronger push away
                separation += diff.Normalized() / distance;
                count++;
            }
        }

        return count > 0 ? separation.Normalized() : Vector2.Zero;
    }

    /// <summary>
    /// Defenders "jockey" the ball carrier - staying between them and goal
    /// without committing to a tackle unless very close.
    /// </summary>
    public Vector2 GetJockeyingForce()
    {
        if (!IsBallPossessedByOpponent() || ball.Carrier == null)
            return Vector2.Zero;

        float distanceToBall = player.Position.DistanceTo(ball.Position);

        // Ã¢ Only jockey if in "defending zone" but not pressing range
        if (distanceToBall < TACKLE_DISTANCE || distanceToBall > INTERCEPTION_RANGE)
            return Vector2.Zero;

        // Ã¢ Position between carrier and own goal
        Vector2 toOwnGoal = player.Position.DirectionTo(player.ownGoal.Position);
        Vector2 toBallCarrier = player.Position.DirectionTo(ball.Carrier.Position);

        // Ã¢ Ideal jockey position: 60-70px from carrier, on goal-side
        float idealDistance = 65f;
        Vector2 jockeyPosition = ball.Carrier.Position + toOwnGoal.Normalized() * idealDistance;

        Vector2 direction = player.Position.DirectionTo(jockeyPosition);
        float currentDistance = player.Position.DistanceTo(jockeyPosition);

        // Ã¢ Only move if too far from ideal position
        if (currentDistance < 15f)
            return Vector2.Zero;

        float weight = Mathf.Clamp(currentDistance / 50f, 0.3f, 0.8f);
        return direction * weight * 0.7f;
    }

    /// <summary>
    /// Moves player to intercept incoming passes.
    /// </summary>
    private Vector2 GetPassInterceptionForce()
    {
        // Only for loose balls (passes in flight)
        if (ball.Carrier != null)
            return Vector2.Zero;

        // Ignore opponent passes
        if (IsBallPossessedByOpponent())
            return Vector2.Zero;

        Vector2 ballVelocity = ball.Velocity;
        if (ballVelocity.LengthSquared() < 4f) // Ball moving too slow
            return Vector2.Zero;

        // â Predict landing position
        Vector2 landingSpot = ball.Position;

        if (ball.Height > 1f)
        {
            // Ball is airborne - calculate landing point
            float timeToGround = ball.HeightVelocity / BallState.Gravity;
            if (timeToGround < 0) timeToGround = 0.1f; // Failsafe

            landingSpot = ball.Position + ballVelocity * timeToGround;
        }
        else
        {
            // Ground pass - predict 0.5 seconds ahead
            landingSpot = ball.Position + ballVelocity * 0.5f;
        }

        float distanceToLanding = player.Position.DistanceTo(landingSpot);

        // Only intercept if within reasonable range
        if (distanceToLanding > 180f)
            return Vector2.Zero;

        // â Move to intercept with urgency
        Vector2 direction = player.Position.DirectionTo(landingSpot);
        float urgency = 1f - (distanceToLanding / 180f);
        urgency = Mathf.Pow(urgency, 1.5f); // Exponential curve

        float awarenessModifier = player.GameAttributes.Awareness / 100f;

        return direction * urgency * awarenessModifier * 1.4f;
    }

    /// <summary>
    /// Defends against shots on goal.
    /// </summary>
    private Vector2 GetShotBlockingForce()
    {
        if (ball.CurrentState is not BallStateShot)
            return Vector2.Zero;

        // Only defend own goal
        if (!ball.IsHeadedForScoringArea(player.ownGoal.GetScoringArea()))
            return Vector2.Zero;

        float distanceToBall = player.Position.DistanceTo(ball.Position);

        // â INCREASED RANGE - defenders react earlier
        if (distanceToBall > 140f) // Was 100f
            return Vector2.Zero;

        // â Calculate interception point based on ball velocity
        Vector2 ballPath = ball.Velocity.Normalized();
        Vector2 toBall = ball.Position - player.Position;
        float projection = toBall.Dot(ballPath);

        if (projection < 0) // Ball moving away
            return Vector2.Zero;

        // â Intercept the PATH, not the ball's current position
        Vector2 interceptPoint = ball.Position + ballPath * Mathf.Min(projection, distanceToBall);
        float distanceToIntercept = player.Position.DistanceTo(interceptPoint);

        // â Only attempt if reachable
        if (distanceToIntercept > 60f)
            return Vector2.Zero;

        Vector2 direction = player.Position.DirectionTo(interceptPoint);

        // â DESPERATE urgency - this is a shot on goal!
        float urgency = 1f - (distanceToIntercept / 60f);
        urgency = Mathf.Pow(urgency, 1.5f); // Aggressive curve

        float defenseModifier = player.GameAttributes.Defense / 100f;
        float awarenessModifier = player.GameAttributes.Awareness / 100f;

        // â HIGHEST PRIORITY - override everything else
        return direction * urgency * defenseModifier * awarenessModifier * 2.5f; // Was 2.0f
    }

    /// <summary>
    /// Recovery run when beaten by attacker.
    /// </summary>
    private Vector2 GetRecoveryRunForce()
    {
        if (!IsBallPossessedByOpponent() || ball.Carrier == null)
            return Vector2.Zero;

        // Check if carrier is between player and own goal
        Vector2 toCarrier = player.Position.DirectionTo(ball.Carrier.Position);
        Vector2 toGoal = player.Position.DirectionTo(player.ownGoal.Position);

        bool beaten = toCarrier.Dot(toGoal) > 0.6f; // Carrier ahead of defender

        if (!beaten)
            return Vector2.Zero;

        // Check if carrier is attacking (moving toward goal)
        Vector2 carrierDirection = ball.Carrier.Velocity.Normalized();
        Vector2 attackDirection = ball.Carrier.Position.DirectionTo(player.ownGoal.Position);
        bool carrierAttacking = carrierDirection.Dot(attackDirection) > 0.5f;

        if (!carrierAttacking)
            return Vector2.Zero;

        // â SPRINT BACK toward defensive position
        Vector2 defensivePosition = player.ownGoal.Position +
            (player.spawnPosition - player.ownGoal.Position).Normalized() * 80f;

        Vector2 direction = player.Position.DirectionTo(defensivePosition);
        float distance = player.Position.DistanceTo(defensivePosition);

        float urgency = Mathf.Clamp(distance / 150f, 0.5f, 1.0f);

        return direction * urgency * 1.5f; // Override all other behaviors
    }
}