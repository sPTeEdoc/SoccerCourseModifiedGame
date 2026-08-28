using Godot;
using System;

[GlobalClass]
public partial class PlayerState : Node
{
    [Signal]
    public delegate void StateTransitionRequestedEventHandler(int nextState, PlayerStateData data);

    public void EmitStateTransition(PlayerCharacter.State nextState)
    {
        EmitSignal("StateTransitionRequested", (int)nextState);
    }
    protected AIBehavior aiBehavior;
    protected Ball ball;
    protected Area2D ballDetectionArea;
    protected Goal ownGoal;
    protected PlayerCharacter player;
    protected PlayerStateData stateData = new PlayerStateData();
    protected Goal targetGoal;
    protected Area2D tackleDamageEmitterArea;
    protected Area2D teammateDetectionArea;

    public void Setup(PlayerCharacter contextPlayer, PlayerStateData contextData,
                      Ball contextBall, Area2D contextTeammateDetectionArea, Area2D contextBallDetectionArea,
                      Goal contextOwnGoal, Goal contextTargetGoal, Area2D contextTackleDamageEmitterArea,
                      AIBehavior contextAIBehavior)
    {
        player = contextPlayer;
        stateData = contextData;
        ball = contextBall;
        teammateDetectionArea = contextTeammateDetectionArea;
        ballDetectionArea = contextBallDetectionArea;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;
        aiBehavior = contextAIBehavior;
        tackleDamageEmitterArea = contextTackleDamageEmitterArea;
    }

    public void TransitionState(PlayerCharacter.State newState, PlayerStateData data = null)
    {
        EmitSignal(SignalName.StateTransitionRequested, (int)newState, data ?? new PlayerStateData());
    }

    public virtual void OnAnimationComplete()
    {
        // Override in derived states
    }

    public virtual bool CanCarryBall() => false;
    public virtual bool CanPass() => false;
    public virtual bool IsReadyForKickoff() => false;
}