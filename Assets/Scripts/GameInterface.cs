using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface : MonoBehaviour
{
    public static GameInterface Interface { get; private set; }
    public EventSystem EventSystem { get; private set; }

    private void Awake() {
        if (Interface != null)
        {
            Destroy(gameObject);
            return;
        }

        Interface = this;
        DontDestroyOnLoad(gameObject);
        EventSystem = new EventSystem();
    }

    private void OnDestroy() {
    }
}
