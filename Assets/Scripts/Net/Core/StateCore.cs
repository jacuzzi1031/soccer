using System.Collections;
using System.Collections.Generic;
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
    OFFENSE
}
public enum BallState {CARRIED, FREEFORM, SHOT}
public enum ControlScheme{ CPU,P1,P2};

public struct SimulationCommand
{
    public SimulationCommandType Type;  
    public Vector2 Direction;            
    public int SeatIndex;
    public bool isHome;
}
public enum SimulationCommandType
{
    ResetAndHomeKickoff,
    ResetAndAwayKickoff,
    AllPlayersReadyForKickoff,
    KickoffStart,
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
    public Vector2 Start;
    public Vector2 End;

    public Vector2 Edge;     // End - Start
    public float EdgeSqr;    // length^2
    public Vector2 Normal;   // outward normal
}
