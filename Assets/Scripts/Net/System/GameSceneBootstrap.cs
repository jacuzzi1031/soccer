using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    bool _frameReady;
    bool _started;
    int _startFrame;

    void Awake()
    {
        GameInterface.Interface
            .GameFrameSyncManager
            .OnFirstFrameArrived += OnFirstFrame;
    }

    void OnDestroy()
    {
        GameInterface.Interface
            .GameFrameSyncManager
            .OnFirstFrameArrived -= OnFirstFrame;
    }

    void OnFirstFrame(int serverFrame)
    {
        if (_started)
            return;

        _started = true;
        _startFrame = serverFrame;

        GameInterface.Interface.GameManager.StartMatch();
    }
}

