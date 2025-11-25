using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationOnComplete : MonoBehaviour
{   
    [SerializeField] private Player player;
    public void OnAnimationComplete()
    {
            if(player != null)
                player.OnAnimationComplete();
    }
}
