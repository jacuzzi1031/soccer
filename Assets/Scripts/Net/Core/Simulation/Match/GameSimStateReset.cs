
    using UnityEngine;

    public class GameSimStateReset:GameSimState {
        

        public override void OnEnter() {
            _commandBuffer.Enqueue(new SimulationCommand
            {
                Type =  _matchSystem.resetAndHomeKickoff()
                    ? SimulationCommandType.ResetAndHomeKickoff
                    : SimulationCommandType.ResetAndAwayKickoff
            });
            _eventBus.Publish(new TeamResetSignal());
        }
        public override void OnExit() {}

        public override void _Update() {
        }
    }
