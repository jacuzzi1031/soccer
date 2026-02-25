using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct CarrierSnapshot
{
    public readonly int Id;
    public readonly Vector2 Position;
    public readonly Vector2 Velocity;
 

    public CarrierSnapshot(PlayerSim playerSim)
    {
        Id = playerSim.playerId;
        Position = playerSim.teamResetPosition;
        Velocity = playerSim.Velocity;

    }
}

