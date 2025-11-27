using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior : MonoBehaviour
{
    public Vector2 moveDir;

    public void UpdateAI()
    {
        perform_ai_movement();
        perform_ai_decisions();
    }

    private void perform_ai_decisions() {
        throw new System.NotImplementedException();
    }

    private void perform_ai_movement() {
        throw new System.NotImplementedException();
    }
}
