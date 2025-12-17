using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CountryConfirmEvent : IEvent
{
    public string Country;

    public CountryConfirmEvent(string countryName) {
        Country = countryName;
    }
}
