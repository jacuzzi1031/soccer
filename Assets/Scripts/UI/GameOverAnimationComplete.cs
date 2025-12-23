using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverAnimationComplete : MonoBehaviour
{
    [SerializeField]GameUI gameUI;
    public void OnAnimationComplete() {
        gameUI.OnGameOverAnimationComplete();
    }
}
