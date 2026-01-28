using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PassBallToSwapPlayer : IEvent
{
    public PlayerView ToSwapPlayerView;
    public PassBallToSwapPlayer(PlayerView toSwapPlayerView)
    {
        ToSwapPlayerView = toSwapPlayerView;
    }
}
