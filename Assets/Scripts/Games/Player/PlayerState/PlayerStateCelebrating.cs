
    using UnityEngine;

    public class PlayerStateCelebrating: PlayerState {
        private const float AIR_FRICTION = 60f;
        private const float CELEBRATING_HEIGHT = 1.5f;
        private float initialDelay;         
        private float startCelebratingTime;
        public override void OnEnter() {
            animator.Play("celebrate");
            GameInterface.Interface.EventSystem.Subscribe<TeamResetEvent>(OnTeamReset);
            initialDelay = Random.Range(0,0.4f);
            startCelebratingTime = Time.time;
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                Vector2.zero,
                Time.deltaTime * AIR_FRICTION
            );
        }

        public override void _Update() {
            if (player.height == 0f && (Time.time - startCelebratingTime) > initialDelay)
            {
                Celebrate();
            }
        }
        private void Celebrate()
        {
            // Godot: animation_player.play("celebrate")
            animator.Play("celebrate");

            // Godot: player.height = 0.1
            player.height = 0.1f;

            // Godot: player.height_velocity = CELEBRATING_HEIGHT
            player.heightVelocity = CELEBRATING_HEIGHT;
        }

        public void OnTeamReset(TeamResetEvent teamReset) {
            TransitionState(Player.State.RESETING,PlayerStateData.Build().SetResetPosition(player.spawnPosition));
        }
    }
