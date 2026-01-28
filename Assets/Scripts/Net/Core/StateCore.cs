using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerStateId
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
    VOLLEY_KICK_OR_HEADER,
}
public enum Role {
    GOALIE,
    DEFENSE,
    MIDFIELD,
    OFFENSE
}
public enum BallStateId {CARRIED, FREEFORM, SHOT}
public enum ControlScheme{ CPU,P1,P2};

public class PlayerInitData
{
    public int ownerPlayerId;
    public bool isHome;
    public ControlScheme controlScheme; 
}
public class ControlContext
{
    public int HomeOwnerId;
    public int AwayOwnerId;

    public int HomeControlledPlayerId;
    public int AwayControlledPlayerId;
}