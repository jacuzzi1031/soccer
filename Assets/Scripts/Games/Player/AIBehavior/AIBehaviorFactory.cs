using System;
using System.Collections;
using System.Collections.Generic;
using Games.Player.AIBehavior;
using UnityEngine;

public class AIBehaviorFactory
{
    private readonly Dictionary<Role, Func<AIBehavior>> _stateFactories =
        new Dictionary<Role, Func<AIBehavior>>
        {
            { Role.DEFENSE, () => new AIBehaviorDefense() },
            { Role.GOALIE, () => new AIBehaviorGoalie() },
            { Role.MIDFIELD, () => new AIBehaviorMidfield() },
            { Role.ATTACKER, () => new AIBehaviorAttacker() },
        };

    public AIBehavior GetFreshAIBehavior(Role role)
    {
        if (_stateFactories.TryGetValue(role, out var factory))
            return factory();

        throw new ArgumentException($"AIBehavior not registered: {role}");
    }
}
