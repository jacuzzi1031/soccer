using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimulationModel
{
    public BallSim BallSim;

    public PlayerSystem PlayerSystem;
    public MatchSystem  MatchSystem;

    public SimulationModel(MatchSystem matchSystem,PlayerSystem playerSystem,BallSim ballSim) {
        BallSim=ballSim;
        PlayerSystem=playerSystem;
        MatchSystem=matchSystem;
    }

    /*
     *如果是纯 State 架构
     *struct SimulationState
        {
            BallState Ball;
            PlayerState[] Players;
        }
     * 每 Tick保存完整 State是浪费
     */
}

