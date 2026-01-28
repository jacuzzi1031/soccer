using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameSync;
using UnityEngine;

public class GameFrameSyncManager : BaseManager
{
    private readonly SortedList<int, ResFrameSyncData> _mHistoryFrameSyncData = new();
    private readonly List<Entity> _mEntities = new();
    private int _serverAuthoritativeFrameId = -1;
    private int _lastExecutedFrameId = -1;
    private bool _initialized;
    private int _serverFrameId;
    public event Action<int> OnFirstFrameArrived;
    
    private ObjectPool<ReqFrameInputData> _mReqFrameInputDataPool;
    private ObjectPool<ReqFrameSyncData> _mReqFrameSyncDataPool;
    
    private ObjectPool<Vector2D> _mVector2DPool;

    public override void OnInit()
    {
        _mReqFrameInputDataPool = new ObjectPool<ReqFrameInputData>(() => new ReqFrameInputData());
        _mVector2DPool = new ObjectPool<Vector2D>(() => new Vector2D());
        GameInterface.Interface.UdpListener.OnReceiveFrameSync += ServerFrameSyncDataUpdate;
        base.OnInit();
    }

    public override void OnDestroy()
    {
        GameInterface.Interface.UdpListener.OnReceiveFrameSync -= ServerFrameSyncDataUpdate;
        base.OnDestroy();
    }

    private void ServerFrameSyncDataUpdate(ResFrameSyncData resFrameSyncData)
    {
        
        _serverFrameId = resFrameSyncData.FrameId;
        if (!_initialized)
        {
            _initialized = true;
            SimulationDriver.Instance.PrepareMatch(_serverFrameId - 1);
            OnFirstFrameArrived?.Invoke(_serverFrameId);
            
            UploadLocalInput(_serverAuthoritativeFrameId);
            return;
        }
        int nextExecuteFrameId = _lastExecutedFrameId + 1;
        if (nextExecuteFrameId <= _serverAuthoritativeFrameId)
        {
            var frameInputs = resFrameSyncData.PlayersFrameInputData
                .Where(i => i.FrameId == nextExecuteFrameId)
                .ToList();
            foreach (var frameInput in frameInputs) {
                SimulationDriver.Instance.InputBuffer.Push(frameInput);
            }
            _lastExecutedFrameId = nextExecuteFrameId;
        }
        UploadLocalInput(_serverAuthoritativeFrameId);
    }
    private void UploadLocalInput(int targetFrameId)
    {
        ReqFrameInputData req = _mReqFrameInputDataPool.Allocate();

        req.FrameId = targetFrameId;
        req.PlayerId = GameInterface.Interface.LocalPlayerInfo.id;
        req.InputType = GameInput.Instance.LocalPlayerInputType;

        Entity localEntity = _mEntities
            .Find(e => e.playerType == Entity.PlayerType.Local);
        var moveVector = _mVector2DPool.Allocate();

        moveVector.X = GameInput.Instance.MoveX;
        moveVector.Y =  GameInput.Instance.MoveY;

        req.MoveVector = moveVector;
        // req.ActiveUnitIndex = localEntity.activeUnitIndex;
        // req.CommandIndex = localEntity.commandIndex;
        

        var sendData = _mReqFrameSyncDataPool.Allocate();
        sendData.FrameId = targetFrameId;
        sendData.ReqFrameInputData = req;
        sendData.RoomCode = GameInterface.Interface.RoomManager.CurrentRoomInfo.roomCode;

        GameInterface.Interface.UdpListener.Send(sendData);
        
                
        _mReqFrameInputDataPool.Release(req);
        _mVector2DPool.Release(moveVector);
    }
    public void AddEntity(in Entity entity)
    {
        _mEntities.Add(entity);
    }
}