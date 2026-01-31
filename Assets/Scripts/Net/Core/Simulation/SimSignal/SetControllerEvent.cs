using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SetControllerEvent : IEvent
{
    public int P1Id { get; }
    public int P2Id { get; }

    public SetControllerEvent(int p1Id, int p2Id)
    {
        P1Id = p1Id;
        P2Id = p2Id;
    }
}
