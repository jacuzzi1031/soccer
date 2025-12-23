using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    public string countryHome;
    public string countryAway;

    public int goalsHome;
    public int goalsAway;

    public string finalScore;
    public string winner;
    
    public Match(string teamHome, string teamAway)
    {
        countryHome = teamHome;
        countryAway = teamAway;
        goalsHome = 0;
        goalsAway = 0;
    }

    public bool IsTied()
    {
        return goalsHome == goalsAway;
    }

    public bool HasSomeoneScored()
    {
        return goalsHome > 0 || goalsAway > 0;
    }
    
    public void IncreaseScore(string countryScoredOn)
    {   
        if (countryScoredOn == countryHome)
            goalsAway += 1;
        else
            goalsHome += 1;

        UpdateMatchInfo();
    }

    public void UpdateMatchInfo()
    {
        winner = goalsHome > goalsAway ? countryHome : countryAway;
        int maxScore = Mathf.Max(goalsHome, goalsAway);
        int minScore = Mathf.Min(goalsHome, goalsAway);
        finalScore = $"{maxScore} - {minScore}";
    }
    
    public void Resolve()
    {
        while (IsTied())
        {
            goalsHome = Random.Range(0, 6); 
            goalsAway = Random.Range(0, 6);
        }

        UpdateMatchInfo();
    }
}
