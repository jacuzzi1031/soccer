using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    [SerializeField] private GameObject entityPrefab;
    private List<RoomPlayerInfo> roomPlayerInfoList;
    
    // int localPlayerId = GameInterface.Interface.LocalPlayerInfo.id;
    private int localPlayerId = 1;
    private void Awake() {
        roomPlayerInfoList=new List<RoomPlayerInfo>();
        RoomPlayerInfo roomPlayerInfo = new RoomPlayerInfo();
        
        //先手动设置
        roomPlayerInfo.id = localPlayerId;
        roomPlayerInfo.isHome = true;
        roomPlayerInfoList.Add(roomPlayerInfo);
        //先手动设置
    }

    private void Start() {
        SetGameMode();
        CreateEntities();
        PlayerManager.Instance.InitializeSquads();
    }

    private void SetGameMode() {
        //应该是创建房间是training，trainingwithEnemy，还是Championship，然后根据roomPlayerInfoList数量是Single，还是Coop，Versus
        if (roomPlayerInfoList.Count == 1) {
            GameManager.Instance.currentGameMode = GameManager.GameMode.Single;
        }
        if (roomPlayerInfoList[0].isHome == roomPlayerInfoList[^1].isHome) {
            GameManager.Instance.currentGameMode = GameManager.GameMode.Coop;
        }
        else {
            GameManager.Instance.currentGameMode = GameManager.GameMode.Versus;
        }
    }

    private void CreateEntities() {
        for (int i = 0; i < roomPlayerInfoList.Count; i++)
        {
            var info = roomPlayerInfoList[i];
            GameObject go = Instantiate(entityPrefab, transform);
            Entity entity = go.GetComponent<Entity>();
            entity.playerType = info.id == localPlayerId ? Entity.PlayerType.Local : Entity.PlayerType.Remote;
            entity.playerId = info.id;
            entity.isHome = info.isHome;
            entity.controlScheme=i==0?Player.ControlScheme.P1:Player.ControlScheme.P2;
        }

    }
}
