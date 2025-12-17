using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OnTeamScoredEvent : IEvent
{
    public string CountryScoredOn;

    public OnTeamScoredEvent(string countryScoredOn)
    {
        CountryScoredOn = countryScoredOn;
    }
}
