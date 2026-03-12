
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
        

        public Sprite GetPlayerSytleSprite(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.VOLLEY_KICK: return volleyKickSprite;
                case PlayerState.HEADER: return headerSprite;
                case PlayerState.BICYCLE_KICK: return bicycleKickSprite;
                case PlayerState.DIVING: return farReachSprite;
                case PlayerState.PASSING: return longpassSrpite;
                case PlayerState.PREPPING_SHOT:
                case PlayerState.SHOOTING: return powershotSprite;
                case PlayerState.TACKLING: return slidetackleSprite;
            }

            return null;
        }
    }
