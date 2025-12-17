using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnImpactReceivedEvent : IEvent
{
    public Vector2 ImpactPosition;
    public bool IsHighImpact;

    public OnImpactReceivedEvent(Vector2 impactPosition, bool isHighImpact)
    {
        ImpactPosition = impactPosition;
        IsHighImpact = isHighImpact;
    }
}
