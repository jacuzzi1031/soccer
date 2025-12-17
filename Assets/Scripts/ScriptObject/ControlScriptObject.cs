using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 逻辑型ScriptableObject适合做“控制模式图标” 因为这三种固定不变。
/// </summary>
[CreateAssetMenu(menuName = "So/Control Scheme")]
public class ControlScriptObject : ScriptableObject
{
    public Sprite cpuSprite;
    public Sprite p1Sprite;
    public Sprite p2Sprite;
    
    public Sprite GetSprite(Player.ControlScheme scheme)
    {
        Sprite result = scheme switch
        {
            Player.ControlScheme.CPU => cpuSprite,
            Player.ControlScheme.P1  => p1Sprite,
            Player.ControlScheme.P2  => p2Sprite,
            _ => null
        };
        return result;
    }

}
