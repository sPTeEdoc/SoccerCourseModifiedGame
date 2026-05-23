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
    protected AnimationPlayer animationPlayer;
    protected Ball ball;
    protected Area2D ballDetectionArea;
    protected Goal ownGoal;
    protected PlayerCharacter player;
    protected PlayerStateData stateData = new PlayerStateData();
    protected Goal targetGoal;
    protected Area2D tackleDamageEmitterArea;
    protected Area2D teammateDetectionArea;

    public void Setup(PlayerCharacter contextPlayer, PlayerStateData contextData)
    {
        player = contextPlayer;
        stateData = contextData;

        // Pull common dependencies directly from the player!
        animationPlayer = player.animationPlayer;
        ball = player.ball;
        teammateDetectionArea = player.teammateDetectionArea;
        ballDetectionArea = player.ballDetectionArea;
        ownGoal = player.ownGoal;
        targetGoal = player.targetGoal;
        aiBehavior = player.currentAIBehavior;

        // Handle specialized dependencies cleanly by casting
        if (player is Outfielder outfielder)
        {
            tackleDamageEmitterArea = outfielder.tackleDamageEmitterArea;
        }
        // If you ever need Goalkeeper specific nodes in a state:
        // else if (player is Goalkeeper goalkeeper) { ... }
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