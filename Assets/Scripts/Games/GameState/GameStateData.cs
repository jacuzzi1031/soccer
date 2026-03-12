using UnityEngine;

public class GameStateData
{
    public bool scoringIsHome { get; private set; }
    
    public static GameStateData Build()
    {
        return new GameStateData();
    }
    public GameStateData SetIsHomeScoring(bool isHome)
    {
        scoringIsHome = isHome;
        return this;
    }
}