using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : BaseManager
{
    public RoomInfo CurrentRoomInfo { get; private set; }
    public bool JoinedRoom { get; private set; }
    public event Action<RoomPlayerInfo> OnRoomPlayerJoin;
    public event Action<RoomPlayerInfo> OnRoomPlayerQuit;
    public event Action<RoomPlayerInfo> OnRoomPlayerReadyChanged;
    public void JoinRoom(RoomInfo roomInfo, List<RoomPlayerInfo> roomPlayerList)
    {
        JoinedRoom = true;
        CurrentRoomInfo = roomInfo;
        RoomPlayerList = roomPlayerList;
    }
    public void JoinNewRoomPlayer(RoomPlayerInfo roomPlayerInfo)
    {
        RoomPlayerList.Add(roomPlayerInfo);

        Debug.Log("其他玩家加入触发OnRoomPlayerJoin");
        OnRoomPlayerJoin?.Invoke(roomPlayerInfo);
    }
    public void QuitRoom(int playerId)
    {
        int localPlayerId = GameInterface.Interface.LocalPlayerInfo.id;
        if (localPlayerId == playerId)
        {
            CurrentRoomInfo = null;
            JoinedRoom = false;
        }

        RoomPlayerInfo roomPlayerInfo = RoomPlayerList.Find(item => item.id == playerId);
        RoomPlayerList.Remove(roomPlayerInfo);

        OnRoomPlayerQuit?.Invoke(roomPlayerInfo);
    }
    public void RoomPlayerReady(int playerId, bool ready)
    {
        RoomPlayerInfo roomPlayerInfo = RoomPlayerList.Find(item => item.id == playerId);
        roomPlayerInfo.ready = ready;
        OnRoomPlayerReadyChanged?.Invoke(roomPlayerInfo);
        bool allReady = RoomPlayerList.All(item => item.ready);
        if (allReady)
        {   //由后端管理
            // OnRoomPlayerAllReady?.Invoke();
        }
    }
    
    
    public List<RoomPlayerInfo> RoomPlayerList { get; private set; }

    public RoomManager() {
        RoomPlayerList = new List<RoomPlayerInfo>();
        
    }

    public void OnEnter() {
        GameInterface.Interface.EventSystem.Subscribe<CountryConfirmEvent>(onCountryConfirm);
    }

    public void OnExit() {
        GameInterface.Interface.EventSystem.Unsubscribe<CountryConfirmEvent>(onCountryConfirm);
    }

    public override void OnInit() {
        //先手动设置
        RoomPlayerInfo roomPlayerInfo = new RoomPlayerInfo();
        roomPlayerInfo.id = 0;
        roomPlayerInfo.nickname = "jacuzzi";
        roomPlayerInfo.isHome = true;
        RoomPlayerList.Add(roomPlayerInfo);
        GameInterface.Interface.LocalPlayerInfo=roomPlayerInfo;
        //先手动设置
        
    }
    private void onCountryConfirm(CountryConfirmEvent obj) {
        
        int localId = GameInterface.Interface.LocalPlayerInfo.id;
        RoomPlayerInfo roomPlayerInfo =RoomPlayerList.Find(roomPlayer=>roomPlayer.id == localId);
        if(roomPlayerInfo.ready==true) return;
        
        RoomPlayer roomPlayer = RoomVisual.Instance._mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo];
        if (roomPlayer.RoomIndex == 0) {
            roomPlayerInfo.isHome = true;
        }
        else {
            roomPlayerInfo.isHome = false;
        }
        
        SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_SELECT);
        
        GameInterface.Interface.GameManager.SetMatchCountry(roomPlayer.RoomIndex,obj.Country);
        
        roomPlayerInfo.ready=true;
        RoomVisual.Instance._mRoomPlayerInfoToRoomPlayerDict[roomPlayerInfo].SetConfirmed(true);
        if (RoomPlayerList.Count == 1) {
            RoomVisual.Instance.SpawnCPU(obj.Country,
                (index, country) => {
                    GameInterface.Interface.GameManager.SetMatchCountry(index,country);
                    SoundManager.Instance.Play(SoundManager.Instance.audioRefs.UI_SELECT);
                } );
            //没给GameManager 信息
        }
        
        //下面是服务端确认
        bool allReady = RoomPlayerList.All(item => item.ready);
        if (allReady)
        {
            GameInterface.Interface.StartCoroutine(DelayLoadScene());
        }

        // if (allReady) {
        //      GameInterface.Interface.SceneLoader.LoadGameSceneAsync();
        // }
    }
    IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1f);
        GameInterface.Interface.SceneLoader.LoadScene(Scene.LoadingScene);
    }
    

    
}
