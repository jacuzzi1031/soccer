using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerViewStateFactory
{   
    [SerializeField] private Sprite volleyKickSprite; 
    [SerializeField] private Sprite bicycleKickSprite; 
    [SerializeField] private Sprite farReachSprite; 
    [SerializeField] private Sprite longballpassSrpite; 
    [SerializeField] private Sprite powershotSprite; 
    [SerializeField] private Sprite headerSprite; 
    [SerializeField] private Sprite slidetackleSprite; 
    
    private readonly Dictionary<PlayerView.State, Func<PlayerViewState>> _stateFactories;

    public PlayerViewStateFactory() {
        _stateFactories= new Dictionary<PlayerView.State, Func<PlayerViewState>> {
            { PlayerView.State.BICYCLE_KICK, () => new PlayerViewStateBicycleKick(bicycleKickSprite) },
            { PlayerView.State.CELEBRATING, () => new PlayerViewViewStateCelebrating() },
            { PlayerView.State.CHEST_CONTROL, () => new PlayerViewViewStateChestControl() },
            { PlayerView.State.DIVING, () => new PlayerViewViewStateDiving(farReachSprite) },
            { PlayerView.State.HURT, () => new PlayerViewViewStateHurt() },
            { PlayerView.State.MOURNING, () => new PlayerViewViewStateMourning() },
            { PlayerView.State.MOVING, () => new PlayerViewViewStateMoving() },
            { PlayerView.State.PASSING, () => new PlayerViewViewStatePassing(longballpassSrpite) },
            { PlayerView.State.PREPPING_SHOT, () => new PlayerViewViewStatePreppingShot(powershotSprite) },
            { PlayerView.State.RESETING, () => new PlayerViewViewStateReseting() },
            { PlayerView.State.RECOVERING, () => new PlayerViewViewStateRecovering() },
            { PlayerView.State.SHOOTING, () => new PlayerViewViewStateShooting() },
            { PlayerView.State.TACKLING, () => new PlayerViewViewStateTackling(slidetackleSprite) },
            { PlayerView.State.VOLLEY_KICK_OR_HEADER, () => new PlayerViewViewStateVolleyKickOrHeader(volleyKickSprite,headerSprite) },
        };
    }

    public PlayerViewState GetFreshState(PlayerView.State state)
    {
        if (_stateFactories.TryGetValue(state, out var factory))
            return factory();

        throw new ArgumentException($"PlayerState not registered: {state}");
    }
}