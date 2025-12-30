
    using System;
    using UnityEngine;


    public class PlayerStatePreppingShot: PlayerState {
        Vector2 shotDirection = Vector2.zero;
        private float timeStartShot;
        private const float DURATION_MAX_BONUS=1.0f;
        private const float EASE_REWARD_FACTOR = 2.0f;
        private float pressRaw;
        private bool hasTriggeredBonusEvent = false;
        public PlayerStatePreppingShot(Sprite sprite) {
            playStyleSprite=sprite;
        }
        public override void OnEnter() {
            animator.Play("pre_kick");
            player.rb.velocity = Vector2.zero;
            
            timeStartShot = Time.time;
            shotDirection=player.headingRight? Vector2.right : Vector2.left;
            
            CameraManager.Instance.PowerShotZoom(true);

            hasTriggeredBonusEvent = false;
        }
        
        public override void OnExit() {
            CameraManager.Instance.PowerShotZoom(false);
            base.OnExit();
        }

        public override void OnShootCancel() {
            if (ball.carrier != player) return;
            float durationPress = Mathf.Clamp(pressRaw, 0f, DURATION_MAX_BONUS);


            
            float easeTime = durationPress / DURATION_MAX_BONUS;
            float bonus = Mathf.Pow(easeTime, EASE_REWARD_FACTOR);
            float shotPower = player.power * (1 + 1.5f*bonus);
            shotDirection = shotDirection.normalized;
            PlayerStateData data = PlayerStateData.Build().SetShotPower(shotPower).SetShotDirection(shotDirection).SetPowerShotSound(hasTriggeredBonusEvent);
            TransitionState(Player.State.SHOOTING, data);
        }

        public override void _Update() {
            shotDirection += GameInput.Instance.GetMovementVector();
            
            pressRaw = Time.time - timeStartShot;
            if (!hasTriggeredBonusEvent && Time.time - timeStartShot >= DURATION_MAX_BONUS/2f) {
                GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,playStyleSprite));
                hasTriggeredBonusEvent = true;
            }
        }

        public override bool CanPass() {
            return true;
        }
    }
