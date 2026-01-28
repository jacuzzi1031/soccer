
    using UnityEngine;

    public class PlayerViewViewStateMourning: PlayerViewState {
        public override void OnEnter() {
            animator.Play("mourn");
            GameInterface.Interface.EventSystem.Subscribe<OnTeamResetEvent>(OnTeamReset);
            rb.velocity = Vector2.zero;
        }
        
        public void OnTeamReset(OnTeamResetEvent teamReset) {
            TransitionState(PlayerView.State.RESETING,PlayerStateData.Build().SetResetPosition(PlayerView.KickoffPosition));
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamReset);
            base.OnExit();
        }
        
    }
