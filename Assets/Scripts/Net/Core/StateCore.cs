using System.Collections;
using System.Collections.Generic;
using GameFrameSync;
using Net.FixFloat;
using UnityEngine;

public enum MatchType {
    Training,          //自由训练
    TrainingWithEnemy,//对抗训练
    UltimateTeam     //正式比赛
}
public enum PlayerState
{
    BICYCLE_KICK,
    CELEBRATING,
    CHEST_CONTROL,
    DIVING,
    HURT,
    MOURNING,
    MOVING,
    PASSING,
    PREPPING_SHOT,
    RESETING,
    RECOVERING,
    SHOOTING,
    TACKLING,
    VOLLEY_KICK,
    HEADER,
}
public enum Role {
    GOALIE,
    DEFENSE,
    MIDFIELD,
    ATTACKER
}
public enum BallState {CARRIED, FREEFORM, SHOT,RESET}
public enum ControlScheme{ CPU,P1,P2};

public struct SimulationCommand
{
    public SimulationCommandType Type;  
    public Vector2D moveVector;            
    public int SeatIndex;
    public bool isHome;
    public FixedVector2 Direction
        => new FixedVector2(moveVector).normalized;
}
public enum SimulationCommandType
{
    ResetAndHomeKickoff,
    ResetAndAwayKickoff,
    AllPlayersReadyForKickoff,
    KickoffStart,
    KickoffEnd,
    ShortPass,
    LongPass,
    ShootPress,
    ShootRelease,
    IncisivePass,
    NoneInputCommand,
    Swap,
    TeamScoring,
    GameOverWinner,
}

public enum MatchState {
    IN_PLAY,
    SCORED,
    RESET,
    KICKOFF,
    OVERTIME,
    GAMEOVER
}
public struct LineSegment
{
    public FixedVector2 Start;
    public FixedVector2 End;

    public FixedVector2 Edge;     // End - Start
    public FixedFloat EdgeSqr;    // length^2
}
