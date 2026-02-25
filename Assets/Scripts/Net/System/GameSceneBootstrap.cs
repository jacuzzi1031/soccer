using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    bool _frameReady;
    bool _startFrameSync=false;
    [SerializeField] BallView ballView;
    [SerializeField] Transform topLeft;
    [SerializeField] Transform topRight;
    [SerializeField] Transform downLeft;
    [SerializeField] Transform downRight;

    void Awake()
    {
        GameInterface.Interface
            .GameFrameSyncManager
            .OnFirstFrameArrived += OnFirstFrame;
    }

    public void Update() {
        if (_startFrameSync) {
            _startFrameSync = false;
            
            List<LineSegment> lineSegments = new List<LineSegment>();
            lineSegments.Add(new LineSegment{Start=topLeft.position, End=topRight.position});
            lineSegments.Add(new LineSegment{Start=downLeft.position, End=downRight.position});
            lineSegments.Add(new LineSegment{Start=topLeft.position, End=downLeft.position});
            lineSegments.Add(new LineSegment{Start=topRight.position, End=downRight.position});
            
            GameInterface.Interface.GameManager.StartMatch(ballView,lineSegments);
        }
    }
    void OnDestroy()
    {
        GameInterface.Interface
            .GameFrameSyncManager
            .OnFirstFrameArrived -= OnFirstFrame;
    }

    void OnFirstFrame(int serverFrame)
    {
        if (_startFrameSync)
            return;
        _startFrameSync = true;
    }
}

