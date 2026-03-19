using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior
{  
    protected Vector2 moveDir=Vector2.zero;
    protected BallSim ballSim;
    protected TriggerDetection opponentDetectionArea;
    protected PlayerSim playerSim;
    private float nextAiTickTime;
    public float aiTimer;
    private const float aiTickFrequency = 0.2f;
    protected Rect GoalArea;
    public int homeCount;
    public int awayCount;
    public int matchPlayerCount;

    public void UpdateAI()
    {
        while (aiTimer >= aiTickFrequency)
        {
            aiTimer -= aiTickFrequency;

            PerformAIMovement();
            PerformAIDecisions();
        }
    }
    public virtual void PerformAIMovement() {
    }
    public virtual void PerformAIDecisions() {
    }
    public Vector2 GetAIMoveDir() {
        return moveDir;
    }

    public void Setup(PlayerSim playerSim, BallSim ballSim,Rect GoalArea,int MatchPlayerCount) {
        this.playerSim=playerSim;
        this.ballSim=ballSim;
        this.GoalArea=GoalArea;
        matchPlayerCount=MatchPlayerCount;
        float r = HashRandom(playerSim.playerId);

        aiTimer = r * aiTickFrequency;
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
