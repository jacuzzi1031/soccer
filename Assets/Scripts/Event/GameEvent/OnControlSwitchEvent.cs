using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnControlSwitchEvent : IEvent {
    public int OldPlayerId; // -1 表示没有旧玩家
    public int NewPlayerId;
    public ControlScheme Scheme;

    public OnControlSwitchEvent(int oldPlayerId, int newPlayerId, ControlScheme scheme)
    {
        OldPlayerId = oldPlayerId;
        NewPlayerId = newPlayerId;
        Scheme = scheme;
    }
}
