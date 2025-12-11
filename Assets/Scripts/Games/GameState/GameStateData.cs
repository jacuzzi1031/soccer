using UnityEngine;

public class GameStateData
{
    public string CountryScoredOn { get; private set; }
    
    public static GameStateData Build()
    {
        return new GameStateData();
    }
    public GameStateData SetCountryScoredOn(string country)
    {
        CountryScoredOn = country;
        return this;
    }
}