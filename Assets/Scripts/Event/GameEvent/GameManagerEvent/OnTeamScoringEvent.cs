using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnTeamScoringEvent : IEvent
{
    public bool isHomeScoring;

    public OnTeamScoringEvent(bool IsHomeScoring)
    {
        isHomeScoring = IsHomeScoring;
    }
}
