using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior
{  
    protected Vector2 moveDir=Vector2.zero;
    protected Ball ball;
    protected TriggerDetection opponentDetectionArea;
    protected Player player;
    private float nextAiTickTime;
    public const float aiTickFrequency = 0.2f;

    public void Start() {
        nextAiTickTime = Time.time + Random.Range(0f, aiTickFrequency);
    }
    public void UpdateAI()
    {
        if (Time.time-nextAiTickTime > aiTickFrequency) {
            nextAiTickTime=Time.time;
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

    public void Setup(Player player, Ball ball, TriggerDetection opponentDetectionArea) {
        this.player=player;
        this.ball=ball;
        this.opponentDetectionArea=opponentDetectionArea;
    }
}
