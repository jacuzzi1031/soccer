using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimStateFactory
{
    private readonly Dictionary<BallStateId, Func<BallSimState>> _stateFactories =
        new Dictionary<BallStateId, Func<BallSimState>>
        {
            { BallStateId.CARRIED, () => new BallSimStateCarried() },
            { BallStateId.FREEFORM, () => new BallSimStateFreeform() },
            { BallStateId.SHOT, () => new BallSimStateShot() },
        };

    public BallSimState GetFreshState(BallStateId ballStateId)
    {
        if (_stateFactories.TryGetValue(ballStateId, out var factory))
            return factory();

        throw new ArgumentException($"Ball state not registered: {ballStateId}");
    }
}
