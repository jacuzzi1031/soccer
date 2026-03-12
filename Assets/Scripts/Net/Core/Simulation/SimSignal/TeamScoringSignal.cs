namespace Net.Core.Simulation.SimSignal {
    public struct TeamScoringSignal {
        public bool isHome;

        public TeamScoringSignal(bool isHome) {
            this.isHome = isHome;
        }
    }
}