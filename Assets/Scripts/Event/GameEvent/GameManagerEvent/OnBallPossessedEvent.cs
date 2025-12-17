using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnBallPossessedEvent : IEvent
{
    public string PlayerName;

    public OnBallPossessedEvent(string playerName)
    {
        PlayerName = playerName;
    }
}
