using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct  CountrySelectEvent:IEvent {
    public int index;
    public string countryName;

    public CountrySelectEvent(int index,string countryName)
    {
        this.index = index;
        this.countryName= countryName;
    }
}
