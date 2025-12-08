using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorFactory
{
    private readonly Dictionary<Player.Role, Func<AIBehavior>> _stateFactories =
        new Dictionary<Player.Role, Func<AIBehavior>>
        {
            { Player.Role.DEFENSE, () => new AIBehaviorField() },
            { Player.Role.GOALIE, () => new AIBehaviorGoalie() },
            { Player.Role.MIDFIELD, () => new AIBehaviorField() },
            { Player.Role.OFFENSE, () => new AIBehaviorField() },
        };

    public AIBehavior GetFreshAIBehavior(Player.Role role)
    {
        if (_stateFactories.TryGetValue(role, out var factory))
            return factory();

        throw new ArgumentException($"AIBehavior not registered: {role}");
    }
}
