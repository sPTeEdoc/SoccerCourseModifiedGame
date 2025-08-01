using Godot;
using System;

[GlobalClass]
public partial class AIBehaviorGoalie : AIBehavior
{
    private const float PROXIMITY_CONCERN = 10.0f;

    public override void PerformAIMovement()
    {
        Vector2 totalSteeringForce = GetGoalieSteeringForce();
        totalSteeringForce = totalSteeringForce.LimitLength(1.0f);
        player.Velocity = totalSteeringForce * player.speed;
    }

    public override void PerformAIDecisions()
    {
        if (ball.IsHeadedForScoringArea(player.ownGoal.GetScoringArea()))
        {
            player.SwitchState(PlayerCharacter.State.DIVING);
        }
    }

    private Vector2 GetGoalieSteeringForce()
    {
        Vector2 top = player.ownGoal.GetTopTargetPosition();
        Vector2 bottom = player.ownGoal.GetBottomTargetPosition();
        Vector2 center = player.spawnPosition;

        float targetY = Mathf.Clamp(ball.Position.Y, top.Y, bottom.Y);
        Vector2 destination = new Vector2(center.X, targetY);
        Vector2 direction = player.Position.DirectionTo(destination);
        float distance = player.Position.DistanceTo(destination);
        float weight = Mathf.Clamp(distance / PROXIMITY_CONCERN, 0, 1);

        return weight * direction;
    }
}