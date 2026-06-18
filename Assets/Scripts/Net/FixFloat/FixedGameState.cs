using System.Collections.Generic;
using Net.FixFloat;

namespace Net.Core.Simulation.SimSignal{
    public class FixedGameState{
        public int Frame;

        public FixedBallState Ball;

        public List<FixedPlayerState> Players;
    }

    public class FixedPlayerState{
        public int playerId;
        public FixedVector2 playerPosition;
        public FixedVector2 playerVelocity;
        public FixedFloat playerHeight;
        public FixedFloat playerHeightVelocity;
        public PlayerState playerState;
        public bool HeadingRight;
    }
    public class FixedBallState{
        public FixedVector2 ballPosition;
        public FixedVector2 ballVelocity;
        public FixedFloat ballHeight;
        public FixedFloat ballHeightVelocity;
        public BallState ballState;
        public int ballCarrierId;
    }
}