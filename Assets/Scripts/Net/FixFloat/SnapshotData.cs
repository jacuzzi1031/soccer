using System.Collections.Generic;
using Net.FixFloat;

namespace Net.Core.Simulation.SimSignal{
    public class SnapshotData
    {
        public int Frame;
        public FixedGameState State;
    }
    public class FixedGameState{
        public int Frame;

        public FixedBallState Ball;

        public List<FixedPlayerState> Players;
        
        //TODO  FixedPlayerStateData,ballStateData
    }

    public class FixedPlayerState{
        public int playerId;
        public FixedVector2 playerPosition;
        public FixedVector2 playerVelocity;
        public FixedFloat playerHeight;
        public FixedFloat playerHeightVelocity;
        public PlayerState playerState;
        public bool HeadingRight;
        public int stateFrame;
    }
    public class FixedBallState{
        public FixedVector2 ballPosition;
        public FixedVector2 ballVelocity;
        public FixedFloat ballHeight;
        public FixedFloat ballHeightVelocity;
        public BallState ballState;
        public int ballCarrierId;
        // 状态内所处帧数
        public int stateFrame;
    }
}