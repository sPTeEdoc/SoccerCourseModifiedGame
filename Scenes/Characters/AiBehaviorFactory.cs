using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class AIBehaviorFactory : GodotObject
{
    private readonly Dictionary<PlayerCharacter.Role, Type> roles = new()
    {
        { PlayerCharacter.Role.DEFENSE, typeof(AIBehaviorField) },
        { PlayerCharacter.Role.GOALIE, typeof(AIBehaviorGoalie) },
        { PlayerCharacter.Role.MIDFIELD, typeof(AIBehaviorField) },
        { PlayerCharacter.Role.OFFENSE, typeof(AIBehaviorField) }
    };

    public AIBehavior GetAIBehavior(PlayerCharacter.Role role)
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