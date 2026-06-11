using System.Collections;
using System.Collections.Generic;
using Net.FixFloat;
using UnityEngine;

public class AIBehavior
{  
    protected FixedVector2 moveDir=FixedVector2.Zero;
    protected BallSim ballSim;
    protected TriggerDetection opponentDetectionArea;
    protected PlayerSim playerSim;
    private FixedFloat nextAiTickTime;
    private int aiFrameCounter = 0;
    private const int aiTickIntervalFrames = 12; 
    protected FixedRect GoalArea;
    public int homeCount;
    public int awayCount;
    public int matchPlayerCount;

    public void UpdateAI()
    {
        aiFrameCounter++;

        if (aiFrameCounter >= aiTickIntervalFrames)
        {
            aiFrameCounter -= aiTickIntervalFrames;

            PerformAIMovement();
            PerformAIDecisions();
        }
    }
    public virtual void PerformAIMovement() {
    }
    public virtual void PerformAIDecisions() {
    }
    public FixedVector2 GetAIMoveDir() {
        return moveDir;
    }

    public void Setup(PlayerSim playerSim, BallSim ballSim, FixedRect GoalArea, int MatchPlayerCount)
    {
        this.playerSim = playerSim;
        this.ballSim = ballSim;
        this.GoalArea = GoalArea;
        matchPlayerCount = MatchPlayerCount;

        float r = HashRandom(playerSim.playerId);

        // 随机初始偏移（0 ~ interval-1 帧）
        aiFrameCounter = (int)(r * aiTickIntervalFrames);
    }
    public float HashRandom(int seed)
    {
        uint x = (uint)seed;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;

        return (x & 0xFFFFFF) / (float)0x1000000;
    }
}
