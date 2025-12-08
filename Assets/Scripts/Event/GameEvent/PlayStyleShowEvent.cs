using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayStyleShowEvent : IEvent
{
    public int playerId;
    public Sprite sprite;
    public PlayStyleShowEvent(int playerId, Sprite sprite) {
        this.playerId = playerId;
        this.sprite = sprite;
    }
}
