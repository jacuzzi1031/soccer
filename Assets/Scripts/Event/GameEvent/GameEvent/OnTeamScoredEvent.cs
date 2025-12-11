using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTeamScoredEvent : IEvent
{
    public string CountryScoredOn;

    public OnTeamScoredEvent(string countryScoredOn)
    {
        CountryScoredOn = countryScoredOn;
    }
}
