using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStateFactory
{   
    [SerializeField] private Sprite volleyKickSprite; 
    [SerializeField] private Sprite bicycleKickSprite; 
    [SerializeField] private Sprite farReachSprite; 
    [SerializeField] private Sprite longballpassSrpite; 
    [SerializeField] private Sprite powershotSprite; 
    [SerializeField] private Sprite headerSprite; 
    [SerializeField] private Sprite slidetackleSprite; 
    
    private readonly Dictionary<Player.State, Func<PlayerState>> _stateFactories;

    public PlayerStateFactory() {
        _stateFactories= new Dictionary<Player.State, Func<PlayerState>> {
            { Player.State.BICYCLE_KICK, () => new PlayerStateBicycleKick(bicycleKickSprite) },
            { Player.State.CELEBRATING, () => new PlayerStateCelebrating() },
            { Player.State.CHEST_CONTROL, () => new PlayerStateChestControl() },
            { Player.State.DIVING, () => new PlayerStateDiving(farReachSprite) },
            { Player.State.HURT, () => new PlayerStateHurt() },
            { Player.State.MOURNING, () => new PlayerStateMourning() },
            { Player.State.MOVING, () => new PlayerStateMoving() },
            { Player.State.PASSING, () => new PlayerStatePassing(longballpassSrpite) },
            { Player.State.PREPPING_SHOT, () => new PlayerStatePreppingShot(powershotSprite) },
            { Player.State.RESETING, () => new PlayerStateReseting() },
            { Player.State.RECOVERING, () => new PlayerStateRecovering() },
            { Player.State.SHOOTING, () => new PlayerStateShooting() },
            { Player.State.TACKLING, () => new PlayerStateTackling(slidetackleSprite) },
            { Player.State.VOLLEY_KICK_OR_HEADER, () => new PlayerStateVolleyKickOrHeader(volleyKickSprite,headerSprite) },
        };
    }

    public PlayerState GetFreshState(Player.State state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"State not registered: {state}");
    }
}