using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimulationContext
{
    int Frame { get; }
    float DeltaTime { get; }
    Vector2 BallPosition { get; }
    int BallOwnerId { get; }
    IReadOnlyList<CarrierSnapshot> Players { get; }
}
