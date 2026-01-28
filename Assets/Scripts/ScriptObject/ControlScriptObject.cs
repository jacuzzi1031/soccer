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
    
    public Sprite GetSprite(ControlScheme scheme)
    {
        Sprite result = scheme switch
        {
            ControlScheme.CPU => cpuSprite,
            ControlScheme.P1  => p1Sprite,
            ControlScheme.P2  => p2Sprite,
            _ => null
        };
        return result;
    }

}
