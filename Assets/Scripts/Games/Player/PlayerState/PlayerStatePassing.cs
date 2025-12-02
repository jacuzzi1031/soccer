using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerStatePassing: PlayerState {
        public override void OnEnter() {
            animator.Play("kick");
        }
        public override void OnAnimationComplete() {
            Player passTarget = null;
            switch (stateData.InputType) {
                case GameInput.PlayerInputType.ShortPass:
                    passTarget = GetShortPassTarget(player, PlayerManager.Instance.currentTeam, stateData.MoveDir);
                    break;
                case GameInput.PlayerInputType.LongPass:
                    passTarget  = GetLongPassTarget(player, PlayerManager.Instance.currentTeam, stateData.MoveDir);
                    break;
                case GameInput.PlayerInputType.IncisivePass:
                    passTarget = GetShortPassTarget(player, PlayerManager.Instance.currentTeam, stateData.MoveDir);
                    break;
            }
            
            if (passTarget == null) {
                Vector2 direction = player.headingRight 
                    ? Vector2.right * player.speed
                    : Vector2.left * player.speed;
                ball.passTo(ball.rb.position + direction,true);
            }else if (stateData.InputType == GameInput.PlayerInputType.ShortPass) {
                ball.passTo(passTarget.rb.position + passTarget.rb.velocity * 0.8f,true);
            }else if (stateData.InputType == GameInput.PlayerInputType.LongPass) {
                ball.passTo(passTarget.rb.position + passTarget.rb.velocity * 0.8f,false);
            }
            TransitionState(Player.State.MOVING);
        }
        static bool IsInScreen(Vector3 worldPos)
        {
            Vector3 vp = Camera.main.WorldToViewportPoint(worldPos);
            return vp.x >= 0 && vp.x <= 1 &&
                   vp.y >= 0 && vp.y <= 1 &&
                   vp.z > 0;
        }

        // 判断是否在朝向 90°以内
        static bool IsWithinAngle(Vector2 toTarget, Vector2 moveDir)
        {
            if (moveDir.sqrMagnitude <= 0.01f)
                return false;

            float dot = Vector2.Dot(toTarget.normalized, moveDir.normalized);
            return dot > 0; 
        }
    
        static bool IsInFront(Player target, Player self)
        {
            float dx = target.transform.position.x - self.transform.position.x;
            return self.headingRight ? dx > 0 : dx < 0;
        }
        static List<Player> GetEligibleTargets(Player self, List<Player> team, Vector2 moveDir)
        {
            List<Player> list = new();

            foreach (var p in team)
            {
                if (p == self) continue;

                Vector2 toTarget = p.transform.position - self.transform.position;

                if (!IsInFront(p, self)) continue;
                if (!IsInScreen(p.transform.position)) continue;
                if (!IsWithinAngle(toTarget, moveDir)) continue;

                list.Add(p);
            }
            return list.OrderBy(p =>
                Vector2.Distance(self.transform.position, p.transform.position)
            ).ToList();
        }
    
        public static Player GetShortPassTarget(Player self, List<Player> team, Vector2 moveDir)
        {
            var list = GetEligibleTargets(self, team, moveDir);
            return list.Count > 0 ? list[0] : null;
        }
    
        public static Player GetLongPassTarget(Player self, List<Player> team, Vector2 moveDir)
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
