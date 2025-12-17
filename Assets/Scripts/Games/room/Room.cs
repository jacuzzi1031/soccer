using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {
    [SerializeField] private GameObject entityPrefab;
    private List<RoomPlayerInfo> roomPlayerInfoList;
    


    private void Awake() {
        List<RoomPlayerInfo> roomPlayerInfoList = GameInterface.Interface.RoomManager.RoomPlayerList;
        //SetGameMode
        if (roomPlayerInfoList.Count == 1) {
            GameInterface.Interface.GameManager.SetGameMode(GameManager.GameMode.Single);
        }
        if (roomPlayerInfoList[0].isHome == roomPlayerInfoList[^1].isHome) {
            GameInterface.Interface.GameManager.SetGameMode(GameManager.GameMode.Coop);
        }
        else {
            GameInterface.Interface.GameManager.SetGameMode(GameManager.GameMode.Versus);
        }
        //CreateEntities
        int localPlayerId = GameInterface.Interface.LocalPlayerInfo.id;
        for (int i = 0; i < roomPlayerInfoList.Count; i++)
        {
            RoomPlayerInfo info = roomPlayerInfoList[i];
            GameObject go = Instantiate(entityPrefab, transform);
            Entity entity = go.GetComponent<Entity>();
            entity.playerType = info.id == localPlayerId ? Entity.PlayerType.Local : Entity.PlayerType.Remote;
            entity.playerId = info.id;
            entity.isHome = info.isHome;
            entity.controlScheme=entity.isHome?Player.ControlScheme.P1:Player.ControlScheme.P2;
        }

    }

    private void Start() {
        PlayerManager.Instance.InitializeSquads();
    }
}
