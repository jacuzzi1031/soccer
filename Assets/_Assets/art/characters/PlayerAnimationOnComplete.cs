using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationOnComplete : MonoBehaviour
{   
    [SerializeField] private PlayerView playerView;
    public void OnAnimationComplete()
    {
            if(playerView != null)
                playerView.OnAnimationComplete();
    }
}
