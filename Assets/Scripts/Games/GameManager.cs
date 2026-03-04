using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameSync;
using SocketProtocol;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager:BaseManager
{

    public RoomMatchType currentMatchType;
    public enum GameMode {
        Single,
        Versus,
        Coop
    }
    public GameMode currentGameMode;
    public string[] playerSetup = { "FRANCE", "" };

    public override void OnInit() {
        currentMatchType = RoomMatchType.Training;
    }
    
    public void SetCurrentMatchType(RoomMatchType matchType) {
        currentMatchType=matchType;
    }
    public void SetMatchCountry(int RoomIndex, string Country) {
        playerSetup[RoomIndex] = Country;
        Debug.Log("Country:"+Country);
    }
    public void SetGameMode(GameMode gameMode) {
        currentGameMode = gameMode;
    }
    
}
