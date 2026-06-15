
    using UnityEngine;

    public class GameSimStateKickoff: GameSimState {
        public override void OnEnter() {
        }

        public override void _Update()
        {
        }


        public override void OnKickoffStart(int seatIndex) {
            bool homeKickoff = _matchSystem.resetAndHomeKickoff();
            bool isHome =
                homeKickoff &&
                seatIndex == 0;

            bool isAway =
                !homeKickoff &&
                seatIndex == 1;

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
