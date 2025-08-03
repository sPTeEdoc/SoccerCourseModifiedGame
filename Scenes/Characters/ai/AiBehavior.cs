using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class AIBehavior : Node
{
    private const int DURATION_AI_TICK_FREQUENCY = 200;

    public Ball ball = null;
    public Area2D opponentDetectionArea = null;
    public PlayerCharacter player = null;
    public Area2D teammateDetectionArea = null;

    public int timeSinceLastAITick = (int)Time.GetTicksMsec();

    public override void _Ready()
    {
        timeSinceLastAITick = (int)Time.GetTicksMsec() + (int)GD.Randi() % DURATION_AI_TICK_FREQUENCY;
    }

    public void Setup(PlayerCharacter contextPlayer, Ball contextBall, Area2D contextOpponentDetectionArea, Area2D contextTeammateDetectionArea)
    {
        player = contextPlayer;
        ball = contextBall;
        opponentDetectionArea = contextOpponentDetectionArea;
        teammateDetectionArea = contextTeammateDetectionArea;
    }

    public void ProcessAI()
    {
        if ((int)Time.GetTicksMsec() - timeSinceLastAITick > DURATION_AI_TICK_FREQUENCY)
        {
            timeSinceLastAITick = (int)Time.GetTicksMsec();
            PerformAIMovement();
            PerformAIDecisions();
        }
    }

    public virtual void PerformAIMovement() { }

    public virtual void PerformAIDecisions() { }

    public float GetBicircularWeight(Vector2 position, Vector2 centerTarget, float innerRadius, float innerWeight, float outerRadius, float outerWeight)
    {
        float distance = position.DistanceTo(centerTarget);
        if (distance > outerRadius)
            return outerWeight;
        else if (distance < innerRadius)
            return innerWeight;
        else
        {
            float t = (distance - innerRadius) / (outerRadius - innerRadius);
            return Mathf.Lerp(innerWeight, outerWeight, t);
        }
    }

    public bool IsBallPossessedByOpponent() =>
        ball.Carrier != null && ball.Carrier.team != player.team;

    public bool IsBallCarriedByTeammate() =>
        ball.Carrier != null && ball.Carrier != player && ball.Carrier.team == player.team;

    public bool HasOpponentsNearby()
    {
        return opponentDetectionArea.GetOverlappingBodies()
            .OfType<PlayerCharacter>()
            .Any(p => p.team != player.team);
    }
}
