using System;
using System.Collections.Generic;

public class PlayerStateFactory
{
    private readonly Dictionary<Player.State, Func<PlayerState>> _stateFactories =
        new Dictionary<Player.State, Func<PlayerState>>
        {
            { Player.State.BICYCLE_KICK, () => new PlayerStateBicycleKick() },
            { Player.State.CELEBRATING, () => new PlayerStateCelebrating() },
            { Player.State.CHEST_CONTROL, () => new PlayerStateChestControl() },
            { Player.State.DIVING, () => new PlayerStateDiving() },
            { Player.State.HURT, () => new PlayerStateHurt() },
            { Player.State.HEADER, () => new PlayerStateHeader() },
            { Player.State.MOURNING, () => new PlayerStateMourning() },
            { Player.State.MOVING, () => new PlayerStateMoving() },
            { Player.State.PASSING, () => new PlayerStatePassing() },
            { Player.State.PREPPING_SHOT, () => new PlayerStatePreppingShot() },
            { Player.State.RESETING, () => new PlayerStateReseting() },
            { Player.State.RECOVERING, () => new PlayerStateRecovering() },
            { Player.State.SHOOTING, () => new PlayerStateShooting() },
            { Player.State.TACKLING, () => new PlayerStateTackling() },
            { Player.State.VOLLEY_KICK, () => new PlayerStateVolleyKick() },
        };

    public PlayerState GetFreshState(Player.State state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"State not registered: {state}");
    }
}