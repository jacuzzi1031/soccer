using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSimStateFactory
{
    private readonly Dictionary<BallState, Func<BallSimState>> _stateFactories =
        new Dictionary<BallState, Func<BallSimState>>
        {
            { BallState.CARRIED, () => new BallSimStateCarried() },
            { BallState.FREEFORM, () => new BallSimStateFreeform() },
            { BallState.SHOT, () => new BallSimStateShot() },
        };

    public BallSimState GetFreshState(BallState ballState)
    {
        if (_stateFactories.TryGetValue(ballState, out var factory))
            return factory();

        throw new ArgumentException($"Ball state not registered: {ballState}");
    }
}
