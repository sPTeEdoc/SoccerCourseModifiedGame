using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class AIBehaviorFactory : GodotObject
{
    private readonly Dictionary<PlayerCharacter.OnFieldPositions, Type> roles = new()
    {
        { PlayerCharacter.OnFieldPositions.DEFENSE, typeof(AIBehaviorField) },
        { PlayerCharacter.OnFieldPositions.GOALIE, typeof(AIBehaviorGoalie) },
        { PlayerCharacter.OnFieldPositions.MIDFIELD, typeof(AIBehaviorField) },
        { PlayerCharacter.OnFieldPositions.FORWARD, typeof(AIBehaviorField) }
    };

    public AIBehavior GetAIBehavior(PlayerCharacter.OnFieldPositions role)
    {
        if (!roles.ContainsKey(role))
            throw new InvalidOperationException($"Role '{role}' doesn't exist!");

        var behaviorType = roles[role];
        var behaviorInstance = Activator.CreateInstance(behaviorType) as AIBehavior;

        if (behaviorInstance == null)
            throw new InvalidCastException($"Failed to instantiate {behaviorType.Name} as AIBehavior.");

        return behaviorInstance;
    }
}