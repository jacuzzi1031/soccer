using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorCursor : MonoBehaviour {
    public int positionId;
    public int currentIndex;
    public Image Indicator_1p;
    public Image Indicator_2p;

    public void Init(int roomPosId, int startIndex=0)
    {
        positionId = roomPosId;
        currentIndex = startIndex;
        Indicator_1p.enabled = (roomPosId == 0);
        Indicator_2p.enabled = (roomPosId != 0);
    }

    public void MoveTo(Transform target)
    {
        transform.position = target.position;
    }
}
