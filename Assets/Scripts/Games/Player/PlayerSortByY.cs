using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSortByY : MonoBehaviour
{
    public int offset = 0;
    public SpriteRenderer playerSpriteRenderer;
    public SpriteRenderer shadowSpriteRenderer;
    void LateUpdate()
    {
        int order = -(int)(transform.position.y * 100) + offset;
        playerSpriteRenderer.sortingOrder = order;
        shadowSpriteRenderer.sortingOrder = order;

    }
}
