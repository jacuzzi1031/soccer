
    using UnityEngine;

    public class PlayerStateMourning: PlayerState {
        public override void OnEnter() {
            animator.Play("mourn");
            GameInterface.Interface.EventSystem.Subscribe<TeamResetEvent>(OnTeamReset);
            rb.velocity = Vector2.zero;
        }
        
        public void OnTeamReset(TeamResetEvent teamReset) {
            TransitionState(Player.State.RESETING,PlayerStateData.Build().SetResetPosition(player.spawnPosition));
        }
        
    }
