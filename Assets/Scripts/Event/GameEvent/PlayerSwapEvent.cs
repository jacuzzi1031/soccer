using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwapEvent : IEvent
{
    public Player player;

    public PlayerSwapEvent(Player p)
    {
        player = p;
    }
}
