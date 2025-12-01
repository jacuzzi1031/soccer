using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorFactory
{
    private readonly Dictionary<Player.Role, Func<AIBehavior>> _stateFactories =
        new Dictionary<Player.Role, Func<AIBehavior>>
        {
            { Player.Role.DEFENSE, () => new AIBehavior() },
            { Player.Role.GOALIE, () => new AIBehavior() },
            { Player.Role.MIDFIELD, () => new AIBehavior() },
            { Player.Role.OFFENSE, () => new AIBehavior() },
        };

    public AIBehavior GetFreshAIBehavior(Player.Role role)
    {
        if (_stateFactories.TryGetValue(role, out var factory))
            return factory();

        throw new ArgumentException($"AIBehavior not registered: {role}");
    }
}
