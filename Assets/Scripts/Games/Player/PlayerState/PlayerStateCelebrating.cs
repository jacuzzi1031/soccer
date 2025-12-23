
    using UnityEngine;

    public class PlayerStateCelebrating: PlayerState {
        private const float AIR_FRICTION = 60f;
        private const float CELEBRATING_HEIGHT = 2f;
        private float initialDelay;         
        private float startCelebratingTime;
        public override void OnEnter() {
            GameInterface.Interface.EventSystem.Subscribe<OnTeamResetEvent>(OnTeamReset);
            initialDelay = Random.Range(0.2f,0.5f);
            startCelebratingTime = Time.time;
        }

        public override void OnExit() {
            GameInterface.Interface.EventSystem.Unsubscribe<OnTeamResetEvent>(OnTeamReset);
            player.height = 0.1f;
            player.heightVelocity=0f;
            base.OnExit();
        }

        public override void _Update() {
            if (player.height == 0f && (Time.time - startCelebratingTime) > initialDelay)
            {
                Celebrate();
            }
        }

        public override void _FixedUpdate() {
            rb.velocity = Vector2.MoveTowards(
                rb.velocity,
                Vector2.zero,
                Time.deltaTime * AIR_FRICTION
            );
        }
        private void Celebrate()
        {
            animator.Play("celebrate");
            player.height = 0.1f;
            player.heightVelocity = CELEBRATING_HEIGHT;
        }

        public void OnTeamReset(OnTeamResetEvent teamReset) {
            TransitionState(Player.State.RESETING,PlayerStateData.Build().SetResetPosition(player.spawnPosition));
        }
    }
