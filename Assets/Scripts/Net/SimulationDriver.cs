
using System;
using System.Collections.Generic;
using System.Data;
using GameFrameSync;
using UnityEngine;


public class SimulationDriver : MonoBehaviour
{
    public static SimulationDriver Instance;

    public const float FRAME_DT = 1f / 30f;
    float accumulator;
    int currentFrame;

    
    readonly List<ISimulationSystem> systems = new();
    public InputBuffer InputBuffer { get; } = new InputBuffer();

    public int CurrentFrame => currentFrame;
    private SimulationContext _simulationContext;
    private SimulationModel _simulationModel;
    private CommandBuffer _commandBuffer;
    private SimEventBus _eventBus;
    private const int INPUT_DELAY = 4;
    public enum SimulationState
    {
        Uninitialized,
        Ready,
        Running,
        Stopped
    }
    public SimulationState state = SimulationState.Uninitialized;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PrepareMatch(int startFrame)
    {
        systems.Clear();
        accumulator = 0f;
        currentFrame = startFrame;
    }





    public void StopMatch()
    {
        foreach (var system in systems)
            system.Stop();

        systems.Clear();
    }

    private void Update()
    {  
        if (state != SimulationState.Running)
            return;
        accumulator += Time.deltaTime;

        while (accumulator >= FRAME_DT)
        {
            StepSimulation();
            accumulator -= FRAME_DT;
        }
    }
    private void StepSimulation()
    {
        InputBuffer.ConsumeFrame(currentFrame, DispatchCommand);
        _simulationContext.BuildFrom(currentFrame, FRAME_DT,_commandBuffer.Consume());
        
        for (int i = 0; i < systems.Count; i++)
        {
            systems[i].Tick(_simulationContext);
        }
        UploadLocalInput(currentFrame+INPUT_DELAY);
        currentFrame++;
        
        _eventBus.Flush();
    }
    
    
    private ObjectPool<ReqFrameInputData> _mReqFrameInputDataPool=new ObjectPool<ReqFrameInputData>(()=>new ReqFrameInputData());
    private ObjectPool<Vector2D> _mVector2DPool = new ObjectPool<Vector2D>(() => new Vector2D());
    private ObjectPool<ReqFrameSyncData> _mReqFrameSyncDataPool = new ObjectPool<ReqFrameSyncData>(() => new ReqFrameSyncData());
    // ReSharper disable Unity.PerformanceAnalysis
    private void UploadLocalInput(int targetFrameId)
    {
        ReqFrameInputData req = _mReqFrameInputDataPool.Allocate();

        req.FrameId = targetFrameId;
        req.OwnerPlayerId = GameInterface.Interface.LocalPlayerInfo.id;
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
        _mReqFrameInputDataPool.Release(req);
        _mVector2DPool.Release(moveVector);
    }
    private void DispatchCommand(InputBuffer.Command cmd)
    {
        var flags = (GameInput.InputEventFlags)cmd.inputType;
        
        if ((flags & GameInput.InputEventFlags.Swap) != 0) {
            EnqueueCommad(SimulationCommandType.Swap, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootRelease) != 0) {
            EnqueueCommad(SimulationCommandType.ShootRelease, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShootPress) != 0) {
            EnqueueCommad(SimulationCommandType.ShootPress, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.ShortPass) != 0) {
            EnqueueCommad(SimulationCommandType.ShortPass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.IncisivePass) != 0) {
            EnqueueCommad(SimulationCommandType.IncisivePass, cmd);
            return;
        }
        if ((flags & GameInput.InputEventFlags.LongPass) != 0) {
            EnqueueCommad(SimulationCommandType.LongPass, cmd);
            return;
        }
        if (flags == GameInput.InputEventFlags.None) {
            EnqueueCommad(SimulationCommandType.NoneInputCommand, cmd);
            return;
        }
    }
    private void EnqueueCommad(SimulationCommandType type, InputBuffer.Command cmd)
    {
        _commandBuffer.Enqueue(new SimulationCommand {
            Type = type,
            Direction = cmd.moveVector,
            PlayerId = cmd.playerId,
            OwnerId = cmd.ownerId
        });
    }

    public void SetAttribute(SimulationModel simModel,CommandBuffer commandBuffer,SimEventBus eventBus,SimulationContext simulationContext,ControlContext controlContext,List<ISimulationSystem> systems) {
        _simulationModel = simModel;
        _commandBuffer=commandBuffer;
        _eventBus=eventBus;
        _simulationContext = simulationContext;
        this.systems.Clear();
        this.systems.AddRange(systems);
        state = SimulationState.Ready;
        InputBuffer.SetPlayerIdToSlot(controlContext);
    }
    public void StartSimulation()
    {
        if (state != SimulationState.Ready)
            throw new Exception("SimulationDriver not ready");

        state = SimulationState.Running;
    }
}


