using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : BaseManager
{
    public RoomInfo CurrentRoomInfo { get; private set; }
    public bool JoinedRoom { get; private set; }
    private Dictionary<int, int> _playerIdToRoomIndex
        = new Dictionary<int, int>();
    public event Action<RoomPlayerInfo> OnRoomPlayerJoin;
    public event Action<RoomPlayerInfo,RoomPlayerInfo> OnRoomPlayerQuit;
    public event Action<RoomPlayerInfo,int,string> OnRoomPlayerSelectCountryChanged;
    public event Action<RoomPlayerInfo, string> OnRoomPlayerCountryConfirmed;
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
    public void QuitRoom(int playerId, int localPlayerId)
    {
        var removeInfo = RoomPlayerList.Find(p => p.id == playerId);
        if (removeInfo == null)
        {
            Debug.LogWarning($"QuitRoom: playerId {playerId} not found");
            return;
        }
        RoomPlayerInfo resetInfo = null;

        if (playerId == localPlayerId)
        {
            CurrentRoomInfo = null;
            JoinedRoom = false;
        }
        else
        {
            resetInfo = RoomPlayerList.Find(p => p.id == localPlayerId);
            if (resetInfo != null)
            {
                resetInfo.ready = false;
            }
        }
        RoomPlayerList.Remove(removeInfo);
        OnRoomPlayerQuit?.Invoke(removeInfo, resetInfo);
    }
    public void RoomPlayerSelectCountry(int playerId,int countryIndex, string countryName) {
        RoomPlayerInfo roomPlayerInfo = RoomPlayerList.Find(item => item.id == playerId);
        OnRoomPlayerSelectCountryChanged?.Invoke(roomPlayerInfo,countryIndex,countryName);
    }
    
    public List<RoomPlayerInfo> RoomPlayerList { get; private set; }

    public RoomManager() {
        RoomPlayerList = new List<RoomPlayerInfo>();
        
    }
    public override void OnInit() {
        GameInterface.Interface.TcpClient.OnClientCloseConnection += TcpClientCloseConnection;
    }

    public override void OnDestroy() {
        GameInterface.Interface.TcpClient.OnClientCloseConnection -= TcpClientCloseConnection;
    }

    private void TcpClientCloseConnection()
    {
        if (CurrentRoomInfo != null)
        {
            Debug.Log("连接中断, 退出房间！");
            QuitRoomRequest quitRoomRequest = GameInterface.Interface.RequestManager.GetRequest<QuitRoomRequest>();
            quitRoomRequest.SendQuitRoomRequest();
        }
    }

    public void RoomPlayerConfirmCountry(int playerId, string countryName)
    {
        RoomPlayerInfo info = RoomPlayerList.Find(p => p.id == playerId);
        if (info == null || info.ready) return;

        info.ready = true;

        OnRoomPlayerCountryConfirmed?.Invoke(info, countryName);

        //下面是服务端确认，并且是tcp异步接收中调用这里，会出现在非主线程中调用了 StartCoroutine报错
        // bool allReady = RoomPlayerList.All(item => item.ready);
        // if (allReady)
        // {
        //     GameInterface.Interface.StartCoroutine(DelayLoadScene());
        // }
    }
    // IEnumerator DelayLoadScene()
    // {
    //     yield return new WaitForSeconds(1f);
    //     GameInterface.Interface.SceneLoader.LoadScene(Scene.LoadingScene);
    // }



}
