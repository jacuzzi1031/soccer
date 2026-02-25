using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayStyleShowEvent : IEvent
{
    public int playerId;
    public PlayerState playerState;
    public PlayStyleShowEvent(int playerId, PlayerState PlayerState) {
        this.playerId = playerId;
        this.playerState = PlayerState;
    }
}
