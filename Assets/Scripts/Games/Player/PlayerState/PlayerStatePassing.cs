using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerStatePassing: PlayerState {
    public PlayerStatePassing(Sprite sprite) {
        playStyleSprite=sprite;
    }
        public override void OnEnter() {
            animator.Play("kick");
            GameInterface.Interface.EventSystem.Publish(new PlayStyleShowEvent(player.playerId,playStyleSprite));
            SoundManager.Instance.Play(SoundManager.Instance.audioRefs.PASS);
        }
        public override void OnAnimationComplete() {
            Player passTarget = null;
            IReadOnlyList<Player> team = player.isHome
                ? PlayerManager.Instance.GetSquad(true)
                : PlayerManager.Instance.GetSquad(false);
            switch (stateData.InputType) {
                case GameFrameSync.InputType.ShortPass:
                    passTarget = GetShortPassTarget(player, team, stateData.MoveDir);
                    break;
                case GameFrameSync.InputType.LongPass:
                    passTarget  = GetLongPassTarget(player, team, stateData.MoveDir);
                    break;
                case GameFrameSync.InputType.IncisivePass:
                    passTarget = GetShortPassTarget(player, team, stateData.MoveDir);
                    break;
            }
            
            Vector2 passDestination;
            Vector2 headingOffset = player.headingRight ? Vector2.right * player.speed : Vector2.left * player.speed;

            if (passTarget == null)
            {
                passDestination = (Vector2)ball.transform.position + headingOffset;
                ball.passTo(passDestination,true,passTarget);
            }
            else
            {
                switch (stateData.InputType)
                {
                    case GameFrameSync.InputType.ShortPass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 0.8f;
                        ball.passTo(passDestination,true,passTarget);
                        break;
                    
                    case GameFrameSync.InputType.LongPass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 0.8f;
                        ball.passTo(passDestination,false,passTarget);
                        break;
                    
                    case GameFrameSync.InputType.IncisivePass:
                        passDestination = (Vector2)passTarget.transform.position + passTarget.rb.velocity * 1.8f;
                        ball.passTo(passDestination,true,passTarget);
                        break;
                }
                if (passTarget != null) {
                    player.StartCoroutine(DelayToSwap(0.2f,passTarget));
                }
            }
            TransitionState(Player.State.MOVING);
        }
        private IEnumerator DelayToSwap(float delay,Player passTarget)
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
    
        static bool IsInFront(Player target, Player self)
        {
            float dx = target.transform.position.x - self.transform.position.x;
            return self.headingRight ? dx > 0 : dx < 0;
        }
        static List<Player> GetEligibleTargets(Player self, IReadOnlyList<Player> team, Vector2 moveDir)
        {
            if(moveDir.sqrMagnitude <= 0.01f) moveDir=self.headingRight ? Vector2.right : Vector2.left;
            
            List<Player> list = new();
            Player closestCpuToBall = null;
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
    
        public static Player GetShortPassTarget(Player self, IReadOnlyList<Player> team, Vector2 moveDir)
        { 
            var list = GetEligibleTargets(self, team, moveDir);
            return list.Count > 0 ? list[0] : null;
        }
    
        public static Player GetLongPassTarget(Player self, IReadOnlyList<Player> team, Vector2 moveDir)
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
