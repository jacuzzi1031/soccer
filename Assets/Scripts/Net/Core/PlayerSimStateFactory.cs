using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSimStateFactory : MonoBehaviour
{
    private readonly Dictionary<PlayerStateId, Func<PlayerSimState>> _stateFactories;

    public PlayerSimStateFactory() {
        _stateFactories= new Dictionary<PlayerStateId, Func<PlayerSimState>> {
            { PlayerStateId.BICYCLE_KICK, () => new PlayerStateBicycleKick() },
            { PlayerStateId.CELEBRATING, () => new PlayerStateCelebrating() },
            { PlayerStateId.CHEST_CONTROL, () => new PlayerStateChestControl() },
            { PlayerStateId.DIVING, () => new PlayerStateDiving() },
            { PlayerStateId.HURT, () => new PlayerStateHurt() },
            { PlayerStateId.MOURNING, () => new PlayerStateMourning() },
            { PlayerStateId.MOVING, () => new PlayerStateMoving() },
            { PlayerStateId.PASSING, () => new PlayerStatePassing() },
            { PlayerStateId.PREPPING_SHOT, () => new PlayerStatePreppingShot() },
            { PlayerStateId.RESETING, () => new PlayerStateReseting() },
            { PlayerStateId.RECOVERING, () => new PlayerStateRecovering() },
            { PlayerStateId.SHOOTING, () => new PlayerStateShooting() },
            { PlayerStateId.TACKLING, () => new PlayerStateTackling() },
            { PlayerStateId.VOLLEY_KICK_OR_HEADER, () => new PlayerStateVolleyKickOrHeader() },
        };
    }

    public PlayerSimState GetFreshState(PlayerStateId state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"PlayerState not registered: {state}");
    }
}
