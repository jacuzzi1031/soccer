using System;
using System.Collections.Generic;

public class BallStateFactory
{
    private readonly Dictionary<Ball.State, Func<BallState>> _stateFactories =
        new Dictionary<Ball.State, Func<BallState>>
        {
            { Ball.State.CARRIED, () => new BallStateCarried() },
            { Ball.State.FREEFORM, () => new BallStateFreeform() },
            { Ball.State.SHOT, () => new BallStateShot() },
        };

    public BallState GetFreshState(Ball.State state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"Ball state not registered: {state}");
    }
}