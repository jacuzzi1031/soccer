
    using UnityEngine;

    public class GameSimStateKickoff: GameSimState {
        public override void OnEnter() {
        }

        public override void _Update(float deltaTime)
        {
        }


        public override void OnKickoffStart(int ownerId) {
            bool homeKickoff = _matchSystem.resetAndHomeKickoff();
            bool isHome =
                homeKickoff &&
                ownerId == _matchSystem._controlContext.HomeOwnerId;

            bool isAway =
                !homeKickoff &&
                ownerId == _matchSystem._controlContext.AwayOwnerId;

            if (!isHome && !isAway)
                return;
            _commandBuffer.Enqueue(new SimulationCommand
            {
                Type = SimulationCommandType.KickoffStart
            });
            _eventBus.Publish(new KickoffStartSignal());//for kickoff whistle
            _matchSystem.SwitchGameState(MatchState.IN_PLAY);
        }
        


    }
