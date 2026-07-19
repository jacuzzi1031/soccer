namespace Net.Core.Simulation.SimSignal {
    public struct PlayerVolleyShotEvent:IEvent {
        public string playerName;

        public PlayerVolleyShotEvent(string playerName) {
            this.playerName = playerName;
        }
    }
}