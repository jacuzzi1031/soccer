
    using UnityEngine;

    public class PlayerStateMourning: PlayerState {
        public override void OnEnter() {
            animator.Play("mourn");
            GameInterface.Interface.EventSystem.Subscribe<OnTeamResetEvent>(OnTeamReset);
            rb.velocity = Vector2.zero;
        }
        
        public void OnTeamReset(OnTeamResetEvent teamReset) {
            TransitionState(Player.State.RESETING,PlayerStateData.Build().SetResetPosition(player.KickoffPosition));
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamReset);
            base.OnExit();
        }
        
    }
