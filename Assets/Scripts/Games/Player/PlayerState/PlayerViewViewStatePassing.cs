using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerViewViewStatePassing: PlayerViewState {
    public PlayerViewViewStatePassing(Sprite sprite) {
        playStyleSprite=sprite;
    }
        public override void OnEnter() {
            animator.Play("kick");
            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(PlayerView.playerId,playStyleSprite));
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.PASS);
        }
        public override void OnAnimationComplete() {
            PlayerView passTarget = null;
            IReadOnlyList<PlayerView> team = PlayerView.isHome
                ? PlayerManager.Instance.GetSquad(true)
                : PlayerManager.Instance.GetSquad(false);
            switch (stateData.InputType) {
                case GameFrameSync.InputType.ShortPass:
                    passTarget = GetShortPassTarget(PlayerView, team, stateData.MoveDir);
                    break;
                case GameFrameSync.InputType.LongPass:
                    passTarget  = GetLongPassTarget(PlayerView, team, stateData.MoveDir);
                    break;
                case GameFrameSync.InputType.IncisivePass:
                    passTarget = GetShortPassTarget(PlayerView, team, stateData.MoveDir);
                    break;
            }
            
            Vector2 passDestination;
            Vector2 headingOffset = PlayerView.headingRight ? Vector2.right * PlayerView.speed : Vector2.left * PlayerView.speed;

            if (passTarget == null)
            {
                passDestination = (Vector2)BallView.transform.position + headingOffset;
                BallView.passTo(passDestination,true,passTarget);
            }
            else
            {
                switch (stateData.InputType)
                {
                    case GameFrameSync.InputType.ShortPass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 0.8f;
                        BallView.passTo(passDestination,true,passTarget);
                        break;
                    
                    case GameFrameSync.InputType.LongPass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 0.8f;
                        BallView.passTo(passDestination,false,passTarget);
                        break;
                    
                    case GameFrameSync.InputType.IncisivePass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 1.8f;
                        BallView.passTo(passDestination,true,passTarget);
                        break;
                }
                if (passTarget != null) {
                    PlayerView.StartCoroutine(DelayToSwap(0.2f,passTarget));
                }
            }
            TransitionState(PlayerView.State.MOVING);
        }
        private IEnumerator DelayToSwap(float delay,PlayerView passTarget)
        {
            yield return new WaitForSeconds(delay);
            GameInterface.Interface.EventSystem.Publish(new PlayerBecomesCarrierEvent(passTarget.playerId));
        }
        static bool IsInScreen(Vector3 worldPos)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(worldPos);
            return vp.x >= 0 && vp.x <= 1 &&
                   vp.y >= 0 && vp.y <= 1 &&
                   vp.z > 0;
        }
        static bool IsWithinAngle(Vector2 toTarget, Vector2 moveDir)
        {
            // if (moveDir.sqrMagnitude <= 0.01f)
            //     return false;

            float dot = Vector2.Dot(toTarget.normalized, moveDir.normalized);
            return dot >  0.7071f; 
        }
    
        static bool IsInFront(PlayerView target, PlayerView self)
        {
            float dx = target.transform.position.x - self.transform.position.x;
            return self.headingRight ? dx > 0 : dx < 0;
        }
        static List<PlayerView> GetEligibleTargets(PlayerView self, IReadOnlyList<PlayerView> team, Vector2 moveDir)
        {
            if(moveDir.sqrMagnitude <= 0.01f) moveDir=self.headingRight ? Vector2.right : Vector2.left;
            
            List<PlayerView> list = new();
            PlayerView closestCpuToBall = null;
            float closestDist = float.MaxValue;
            foreach (var p in team)
            {
                if (p == self) continue;
                
                Vector2 toTarget = p.transform.position - self.transform.position;

                // if (!IsInFront(p, self)) continue;
                if (!IsInScreen(p.transform.position)) continue;
                float dist = (p.transform.position - self.transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestCpuToBall = p;
                }
                if (!IsWithinAngle(toTarget, moveDir)) continue;

                list.Add(p);
            }

            if (list.Count == 0) {
                list.Add(closestCpuToBall);
            }
            return list
                .Where(p => p != null && p.transform != null)
                .OrderBy(p => Vector2.Distance(self.transform.position, p.transform.position))
                .ToList();
        }
    
        public static PlayerView GetShortPassTarget(PlayerView self, IReadOnlyList<PlayerView> team, Vector2 moveDir)
        { 
            var list = GetEligibleTargets(self, team, moveDir);
            return list.Count > 0 ? list[0] : null;
        }
    
        public static PlayerView GetLongPassTarget(PlayerView self, IReadOnlyList<PlayerView> team, Vector2 moveDir)
        {
            var list = GetEligibleTargets(self, team, moveDir);

            if (list.Count >= 2)
                return list[1];
            else if (list.Count == 1)
                return list[0];
            else
                return null;
        }
    }
