using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour {
    [SerializeField] private GameObject entityPrefab;
    private List<RoomPlayerInfo> roomPlayerInfoList;
    private List<Entity> entityList= new List<Entity>();
    public static EntityManager Instance{get; private set;}

    private void Awake() {
        Instance=this;
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
            entity.controlScheme=entity.isHome?ControlScheme.P1:ControlScheme.P2;
            entityList.Add(entity);
        }
    }

    public List<int> GetEntityIdListIsHome(bool isHome) {
        List<int> entityisHomeList=new List<int>();
        for (int i = 0; i < entityList.Count; i++) {
            if (entityList[i].isHome == isHome) {
                entityisHomeList.Add(entityList[i].playerId);
            }
        }
        return entityisHomeList;
    }

    public int GetEntityByID() {
        return entityList[0].playerId;
    }
    #region for PauseGame
    private bool isGamePaused = false;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public void OnEnable() {
        GameInput.Instance.OnPauseAction+= OnPauseAction;
    }
    private void OnPauseAction(object sender, EventArgs e) {
        TogglePauseGame();
    }
    public void TogglePauseGame() {
        isGamePaused=!isGamePaused;
        if (isGamePaused) {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion
}
