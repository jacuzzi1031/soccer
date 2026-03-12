using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameSync;
using UnityEngine;

public class GameFrameSyncManager : BaseManager
{
    public InputBuffer InputBuffer { get; private set; }
    public int playerCount{ get; private set; }
    public override void OnInit()
    {
        GameInterface.Interface.UdpListener.OnReceiveFrameSync += ServerFrameSyncDataUpdate;
        InputBuffer = new InputBuffer();
    }
    public void PrepareControlContext() {
        var RoomPlayerList = GameInterface.Interface.RoomManager.RoomPlayerList;
        int playerCount = 0;
        foreach (var info in RoomPlayerList)
        {
            if (info.isHome) {
                playerCount++;
            }
            else {
                playerCount++;
            }
        }
        InputBuffer.SetmaxPlayers(playerCount);
        this.playerCount = playerCount;
    }

    public void ClearInputBuffer() {
        InputBuffer?.Clear();
    }

    public override void OnDestroy()
    {
        GameInterface.Interface.UdpListener.OnReceiveFrameSync -= ServerFrameSyncDataUpdate;
        base.OnDestroy();
    }
    public int _latestServerFrame = -1;
    private void ServerFrameSyncDataUpdate(ResFrameSyncData res)
    {
        _latestServerFrame = res.FrameId;
        foreach (var frameInput in res.PlayersFrameInputData)
        {
            InputBuffer.Push(frameInput);
        }
    }
}