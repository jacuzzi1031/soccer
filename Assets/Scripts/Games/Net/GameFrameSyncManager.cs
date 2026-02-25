using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameSync;
using UnityEngine;

public class GameFrameSyncManager : BaseManager
{
    private bool _initialized=false;
    private int _serverFrameId;
    public event Action<int> OnFirstFrameArrived;
    private ObjectPool<ReqFrameSyncData> _mReqFrameSyncDataPool;
    

    public override void OnInit()
    {
        GameInterface.Interface.UdpListener.OnReceiveFrameSync += ServerFrameSyncDataUpdate;
        base.OnInit();
    }

    public override void OnDestroy()
    {
        GameInterface.Interface.UdpListener.OnReceiveFrameSync -= ServerFrameSyncDataUpdate;
        base.OnDestroy();
    }

    private void ServerFrameSyncDataUpdate(ResFrameSyncData res)
    {
        _serverFrameId = res.FrameId;
        if (!_initialized)
        {
            _initialized = true;
            SimulationDriver.Instance.PrepareMatch(_serverFrameId);
            OnFirstFrameArrived?.Invoke(_serverFrameId);
        }
        
        foreach (var frameInput in res.PlayersFrameInputData)
        {
            SimulationDriver.Instance.InputBuffer.Push(frameInput);
        }
    }
}