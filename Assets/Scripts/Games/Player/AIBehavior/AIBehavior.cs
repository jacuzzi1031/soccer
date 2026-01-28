using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior
{  
    protected Vector2 moveDir=Vector2.zero;
    protected BallView BallView;
    protected TriggerDetection opponentDetectionArea;
    protected PlayerView PlayerView;
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

    public void Setup(PlayerView playerView, BallView ballView, TriggerDetection opponentDetectionArea) {
        this.PlayerView=playerView;
        this.BallView=ballView;
        this.opponentDetectionArea=opponentDetectionArea;
    }
}
