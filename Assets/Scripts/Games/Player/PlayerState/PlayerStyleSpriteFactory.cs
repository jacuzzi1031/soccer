
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    [System.Serializable]
    public class PlayerStyleSpriteFactory {
        [SerializeField] private Sprite volleyKickSprite; 
        [SerializeField] private Sprite bicycleKickSprite; 
        [SerializeField] private Sprite farReachSprite; 
        [SerializeField] private Sprite longpassSrpite; 
        [SerializeField] private Sprite powershotSprite; 
        [SerializeField] private Sprite headerSprite; 
        [SerializeField] private Sprite slidetackleSprite; 
        
        private readonly Dictionary<PlayerState, Sprite> _spriteFactories;

        public PlayerStyleSpriteFactory() {
            _spriteFactories = new Dictionary<PlayerState, Sprite> {
                { PlayerState.VOLLEY_KICK, volleyKickSprite },
                { PlayerState.HEADER, headerSprite },
                { PlayerState.BICYCLE_KICK, bicycleKickSprite },
                { PlayerState.DIVING, farReachSprite },
                { PlayerState.PASSING, longpassSrpite },
                { PlayerState.SHOOTING, powershotSprite },
                { PlayerState.TACKLING, slidetackleSprite },
            };
        }

        public Sprite GetPlayerSytleSprite(PlayerState state)
        {
            if (_spriteFactories.TryGetValue(state, out var sprite))
                return sprite;

            throw new ArgumentException($"PlayerSytle not registered: {state}");
        }
    }
