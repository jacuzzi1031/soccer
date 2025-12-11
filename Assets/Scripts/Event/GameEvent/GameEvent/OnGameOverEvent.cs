using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGameOverEvent : IEvent
{
    public string CountryWinner;

    public OnGameOverEvent(string countryWinner)
    {
        CountryWinner = countryWinner;
    }
}
