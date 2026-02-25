using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSim:ISimulationSystem
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float height=0.0f;
    public float heightVelocity=0.0f;
    private const float KICKOFF_PASS_DISTANCE = 30f;
    private const float DURATION_PASS_LOCK = 0.2f;
    public const float GRAVITY = 8f;
    public Vector2 spawnPosition;
    public BallSimState _ballSimState;
    public BallSimStateFactory stateFactory=new BallSimStateFactory();
    private bool _running;
    public SimEventBus _eventBus;
    public int carrierId=-1;
    public BallState ballState;
    public CommandBuffer _commandBuffer;
    public int Frame { get;private set;}
    public float frictionAir = 1.94f;
    public float frictionGround = 10f;
    public PlayerSim carrier;
    public List<LineSegment> lines;
    public float radius=4.68f;
    public BallSim(Vector2 ContextSpawnPosition,SimEventBus eventBus,CommandBuffer commandBuffer,List<LineSegment> lineSegments)
    {
        spawnPosition = ContextSpawnPosition;
        _eventBus = eventBus;
        Position=spawnPosition;
        _commandBuffer = commandBuffer;
        lines=lineSegments;
        SwitchState(BallState.FREEFORM);
    }
    public void SwitchState(BallState type, BallStateData data = null) {
        if (ballState == type) return;
        _ballSimState?.OnExit();
        ballState = type;
        _ballSimState = stateFactory.GetFreshState(type);
        _ballSimState.Setup(this, data ?? new BallStateData(),_eventBus);
        _ballSimState.OnEnter();
    }

    public void Tick(SimulationContext context) {
        Frame=context.Frame;
        foreach (var command in context.Commands)
        {
            switch (command.Type)
            {
                case SimulationCommandType.ResetAndHomeKickoff:
                case SimulationCommandType.ResetAndAwayKickoff:
                    Position = spawnPosition;
                    Velocity = Vector2.zero;
                    height=0.0f;
                    SwitchState(BallState.FREEFORM);
                    break;
                case SimulationCommandType.KickoffStart:
                    passTo(spawnPosition + Vector2.down * KICKOFF_PASS_DISTANCE, true);
                    break;
                case SimulationCommandType.BallShoot:
                    shoot(command.ShotVelocity);
                    break;
            }
        }
        _ballSimState?._Update(context.DeltaTime);
    }

    private void shoot(Vector2 ShotVelocity) {
        Velocity = ShotVelocity;
        carrier = null;
        SwitchState(BallState.SHOT);
    }

    public void passTo(Vector2 destination,bool overground, float lockDuration = DURATION_PASS_LOCK) {
        Vector2 direction = (destination - Position).normalized;
        float distance = Vector2.Distance(Position, destination);
        float intensity = Mathf.Sqrt(2f * distance * frictionGround);
        Velocity = intensity * direction;

        // 如果是高空的，视为水平是匀速，速度为原本速度intensity  x=Vx*t  t=x/Vx
        // 垂直方向终点y=0,Vy=gt/2  代入t Vy=gx/2Vx 高度增加 /2->/1.85 也会有更快速度，飞到球员脸上而不是脚下
        if (!overground)
        {
            heightVelocity = GRAVITY * distance / (1.85f * intensity);
        }
        else
        {
            heightVelocity = 0f;
        }
        carrier = null;
        carrierId = -1;
        SwitchState(BallState.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
    }
    public bool CanAirInteract() {
        return _ballSimState != null && _ballSimState.CanAirInteract();
    }
}
