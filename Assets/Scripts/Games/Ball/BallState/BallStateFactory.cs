using System;
using System.Collections.Generic;

public class BallStateFactory
{
    private readonly Dictionary<BallStateId, Func<BallViewState>> _stateFactories =
        new Dictionary<BallStateId, Func<BallViewState>>
        {
            { BallStateId.CARRIED, () => new BallViewStateCarried() },
            { BallStateId.FREEFORM, () => new BallViewStateFreeform() },
            { BallStateId.SHOT, () => new BallViewStateShot() },
        };

    public BallViewState GetFreshState(BallStateId ballStateId)
    {
        if (_stateFactories.TryGetValue(ballStateId, out var factory))
            return factory();

        throw new ArgumentException($"Ball state not registered: {ballStateId}");
    }
}