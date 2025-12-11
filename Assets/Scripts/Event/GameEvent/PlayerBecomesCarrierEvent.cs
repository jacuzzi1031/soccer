using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBecomesCarrierEvent : IEvent
{
    public int playerId;

    public PlayerBecomesCarrierEvent(int playerId)
    {
        this.playerId = playerId;
    }
}
