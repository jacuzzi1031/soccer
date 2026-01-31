using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// PlayerSystem 唯一可以推进帧的地方（Tick）
/// PlayerManager 可以在任意时间响应事件 / 表现
/// </summary>
public class PlayerSystem:ISimulationSystem
{
    private List<PlayerSim> teamHome;
    private List<PlayerSim> teamAway;
    private int weightCacheIntervalFrames;
    private const float DURATION_WEIGHT_CACHE = 0.2f;
    private const int FP = 1000;
    private float lastCacheRefreshFrame = 0f;
    private bool isCheckingForKickoffReadiness=false;
    public InputBuffer InputBuffer;
    public ControlContext _control;
	public SimEventBus _eventBus;
	public PlayerSystem(SimEventBus eventBus) {
        _eventBus = eventBus;
    }

    public void RegisterTeams(
        List<PlayerSim> home,
        List<PlayerSim> away)
    {
        teamHome = home;
        teamAway = away;
        weightCacheIntervalFrames=Mathf.RoundToInt(SimulationDriver.FRAME_DT * DURATION_WEIGHT_CACHE);
    }
    public void Tick(ISimulationContext context)
    {
        if (lastCacheRefreshFrame < 0 ||
            context.Frame - lastCacheRefreshFrame >= weightCacheIntervalFrames)
        {
            lastCacheRefreshFrame = context.Frame;
            SetOnDutyWeights(context.BallPosition);
        }
        if (isCheckingForKickoffReadiness)
        {
            CheckForKickoffReadiness();
        }
    }
    void CheckForKickoffReadiness()
    {
        foreach (var squad in new[] { teamHome, teamAway })
        {
            foreach (PlayerSim player in squad)
            {
                if (!player.IsReadyForKickoff())
                {
                    return;
                }
            }
        }
        isCheckingForKickoffReadiness = false;
        ResetControlSchemesSim();
    }

    private void ResetControlSchemesSim() {
        for (int i = 0; i < teamHome.Count; i++) {
            teamHome[i].controlScheme = ControlScheme.CPU;
        }
        for (int i = 0; i < teamAway.Count; i++) {
            teamAway[i].controlScheme = ControlScheme.CPU;
        }

        if (_control.HomeOwnerId != -1) {
            teamHome[^1].controlScheme = ControlScheme.P1;
        }        
        if (_control.AwayOwnerId != -1) {
            teamAway[^1].controlScheme = ControlScheme.P2;
        }
        _eventBus.Publish(new ControllerChangedSignal
        {
            HomePlayerId = _control.HomeOwnerId != -1?teamHome[^1].playerId:-1,
            AwayPlayerId = _control.AwayOwnerId != -1?teamAway[^1].playerId:-1
        });
    }
    private void SetOnDutyWeights(Vector2 ballPosition) {
        List<List<PlayerSim>> squads = new List<List<PlayerSim>> { teamHome, teamAway };

        foreach (var squad in squads)
        {   
            List<PlayerSim> cpuPlayers=null;
            for (int j = 0; j < squad.Count; j++)
            {
                var p = squad[j];
                if (p.controlScheme == ControlScheme.CPU &&
                    p.role != Role.GOALIE)
                {
                    cpuPlayers.Add(p);
                }
            }
            cpuPlayers.Sort((p1, p2) =>
            {
                float d1 = (p1.playerPosition - ballPosition).sqrMagnitude;
                float d2 = (p2.playerPosition - ballPosition).sqrMagnitude;

                int cmp = d1.CompareTo(d2);
                if (cmp != 0)
                    return cmp;
                //tie-breaker = 当主要排序条件“相等或几乎相等”时，用一个“永远一致的次级规则”来打破平局。
                return p1.playerId.CompareTo(p2.playerId);
            });


            for (int i = 0; i < cpuPlayers.Count; i++)
            {
                int x = i * FP / 10;              // 0 ~ 1000
                int eased = (x * x) / FP;         // bias = 2
                int weight = FP - eased;          // 1 - ease

                cpuPlayers[i].weightOnDutySteering = weight;
            }
        }
    }
    public void SetInputBuffer(InputBuffer inputBuffer) {
        this.InputBuffer = inputBuffer;
    }
    public void Stop()
    {
    }

    public void InjectControlContext(ControlContext controlContext) {
        _control=controlContext;
        ResetControlSchemesSim();
    }

    public void ResetTeams() {
        isCheckingForKickoffReadiness = true;
    }
}
