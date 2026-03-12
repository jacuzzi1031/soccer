using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimStateFactory
{
    private readonly Dictionary<PlayerState, Func<PlayerSimState>> _stateFactories;

    public PlayerSimStateFactory() {
        _stateFactories= new Dictionary<PlayerState, Func<PlayerSimState>> {
            { PlayerState.BICYCLE_KICK, () => new PlayerStateBicycleKick() },
            { PlayerState.CELEBRATING, () => new PlayerStateCelebrating() },
            { PlayerState.CHEST_CONTROL, () => new PlayerStateChestControl() },
            { PlayerState.DIVING, () => new PlayerStateDiving() },
            { PlayerState.HURT, () => new PlayerStateHurt() },
            { PlayerState.MOURNING, () => new PlayerStateMourning() },
            { PlayerState.MOVING, () => new PlayerStateMoving() },
            { PlayerState.PASSING, () => new PlayerStatePassing() },
            { PlayerState.PREPPING_SHOT, () => new PlayerStatePreppingShot() },
            { PlayerState.RESETING, () => new PlayerStateReseting() },
            { PlayerState.RECOVERING, () => new PlayerStateRecovering() },
            { PlayerState.SHOOTING, () => new PlayerStateShooting() },
            { PlayerState.TACKLING, () => new PlayerStateTackling() },
            { PlayerState.VOLLEY_KICK, () => new PlayerStateVolleyKick() },
            { PlayerState.HEADER, () => new PlayerStateHeader() },
        };
    }

    public PlayerSimState GetFreshState(PlayerState state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"PlayerState not registered: {state}");
    }
}
