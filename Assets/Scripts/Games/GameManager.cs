using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get;private set; }
    
    
    public Match currentMatch;
    public string[] playerSetup = { "FRANCE", "" };
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentMatch = new Match("FRANCE", "SPAIN");
    }

    private void Start() {
        
    }

    public bool IsCoop() {
        return playerSetup[0]==playerSetup[1];
    }

    public bool IsSinglePlayer() {
        return playerSetup[1]=="";
    }
}
