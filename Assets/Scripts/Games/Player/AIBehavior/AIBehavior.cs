using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior
{  
    private Vector2 moveDir;
    private Ball ball;
    private TriggerDetection opponentDetectionArea;
    private TriggerDetection teammateDetectionArea;
    private Player player;
    public void UpdateAI()
    {
        PerformAIMovement();
        PerformAIDecisions();
    }
    private void PerformAIMovement() {
    }
    private void PerformAIDecisions() {
    }
    public Vector2 GetAIMoveDir() {
        return moveDir;
    }

    public void Setup(Player player, Ball ball, TriggerDetection opponentDetectionArea, TriggerDetection teammateDetectionArea) {
        this.player=player;
        this.ball=ball;
        this.opponentDetectionArea=opponentDetectionArea;
        this.teammateDetectionArea=teammateDetectionArea;
    }
}
