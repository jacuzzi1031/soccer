using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameSync;
using UnityEngine;

public class SimulationClock : MonoBehaviour
{
    public const float FRAME_DT = 1f / 60f;

    public static SimulationClock Instance;

    private float accumulator;
    private int currentFrame;
    private int _startFrame;
    private long _startTime;
    private bool running;
    private bool _waitingStart;

    private SimulationWorld world;
    const int ROLLBACK_STEPS_PER_FRAME = 16;

    public int CurrentFrame => currentFrame;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private  void Start() {
        GameInterface.Interface.UdpListener.OnReceiveFrameDismatch += HandleMismatch;
    }
    private void OnDestroy()
    {
        if (GameInterface.Interface?.UdpListener != null)
        {
            GameInterface.Interface.UdpListener.OnReceiveFrameDismatch -= HandleMismatch;
        }
    }
    private bool _isRollbacking;
    private int rollbackTargetFrame;
    private void HandleMismatch(ResFrameSyncData res)
    {
        if (_isRollbacking)
            return;

        rollbackTargetFrame = currentFrame;

        int snapshotFrame =
            world.LoadNearestSnapshot(
                res.Desync.FrameId);

        if (snapshotFrame < 0)
        {
            Debug.LogError(
                $"Rollback aborted. No snapshot for frame {res.Desync.FrameId}");
            return;
        }
        currentFrame = snapshotFrame;
        _isRollbacking = true;
    }


    public void SetWorld(SimulationWorld simulationWorld)
    {
        world = simulationWorld;
    }

    public void OnGameOver() {
        _waitingStart = false;  
        running = false;
        world = null;
    }

    public void OnStartGame(long startTime, int startFrame) {
        _startFrame = startFrame;
        _startTime = startTime;
        _waitingStart = true;
        running = true;
    }
    private void Update()
    {
        if (_waitingStart) {
            if (TimeSyncManager.Instance.GetServerTimeMs() >= _startTime)
            {
                _waitingStart = false;
                currentFrame = _startFrame;
            }
            return;
        }

        if (!running)
            return;
        accumulator += Time.deltaTime;

        while (accumulator >= FRAME_DT)
        {
            Step();
            accumulator -= FRAME_DT;
        }
        if (_isRollbacking)
        {
            int count = ROLLBACK_STEPS_PER_FRAME;

            while (_isRollbacking && count-- > 0)
            {
                Step();
            }
        }
    }

    private void Step()
    {
        if (!_isRollbacking) {
            UploadLocalInput(currentFrame+INPUT_DELAY);
        }
        if (currentFrame <= GameInterface.Interface.GameFrameSyncManager._latestServerFrame)
        {
            world?.Step(currentFrame,_isRollbacking);
            currentFrame++;
        }

        if (_isRollbacking && currentFrame > rollbackTargetFrame) {
            _isRollbacking = false;
        }
    }
    private const int INPUT_DELAY = 8;
    private ObjectPool<ReqFrameInputData> _mReqFrameInputDataPool=new ObjectPool<ReqFrameInputData>(()=>new ReqFrameInputData());
    private ObjectPool<Vector2D> _mVector2DPool = new ObjectPool<Vector2D>(() => new Vector2D());
    private ObjectPool<ReqFrameSyncData> _mReqFrameSyncDataPool = new ObjectPool<ReqFrameSyncData>(() => new ReqFrameSyncData());

    private void UploadLocalInput(int targetFrameId)
    {
        ReqFrameInputData req = _mReqFrameInputDataPool.Allocate();

        req.FrameId = targetFrameId;
        req.SeatIndex = GameInterface.Interface.RoomManager.localSeatIndex;
        req.InputType = GameInput.Instance.ConsumeEventFlags();
        var moveVector = _mVector2DPool.Allocate();
        
        moveVector.X = GameInput.Instance.MoveX;
        moveVector.Y =  GameInput.Instance.MoveY;
        
        req.MoveVector = moveVector;
        var sendData = _mReqFrameSyncDataPool.Allocate();
        sendData.FrameId = targetFrameId;
        sendData.ReqFrameInputData = req;
        sendData.RoomCode = GameInterface.Interface.RoomManager.CurrentRoomInfo.roomCode;

        GameInterface.Interface.UdpListener.Send(sendData);
        _mReqFrameSyncDataPool.Release(sendData);
        _mReqFrameInputDataPool.Release(req);
        _mVector2DPool.Release(moveVector);
    }

    
}
