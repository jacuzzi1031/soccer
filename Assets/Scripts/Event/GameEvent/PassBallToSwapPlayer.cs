using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PassBallToSwapPlayer : IEvent
{
    public Player ToSwapPlayer;
    public PassBallToSwapPlayer(Player toSwapPlayer)
    {
        ToSwapPlayer = toSwapPlayer;
    }
}
