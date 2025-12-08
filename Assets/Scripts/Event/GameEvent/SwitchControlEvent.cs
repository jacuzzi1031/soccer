using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchControlEvent : IEvent
{
    public int FromPlayerId;
    public int ToPlayerId;
    public Player.ControlScheme NewControlScheme;
    // public int UserId;

    public SwitchControlEvent(int fromId, int toId, Player.ControlScheme scheme)
    {
        FromPlayerId = fromId;
        ToPlayerId = toId;
        NewControlScheme = scheme;
    }
}
