using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehaviorFactory
{
    private readonly Dictionary<Role, Func<AIBehavior>> _stateFactories =
        new Dictionary<Role, Func<AIBehavior>>
        {
            { Role.DEFENSE, () => new AIBehaviorField() },
            { Role.GOALIE, () => new AIBehaviorGoalie() },
            { Role.MIDFIELD, () => new AIBehaviorField() },
            { Role.OFFENSE, () => new AIBehaviorField() },
        };

    public AIBehavior GetFreshAIBehavior(Role role)
    {
        if (_stateFactories.TryGetValue(role, out var factory))
            return factory();

        throw new ArgumentException($"AIBehavior not registered: {role}");
    }
}
