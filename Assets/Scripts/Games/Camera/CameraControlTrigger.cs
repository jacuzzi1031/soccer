using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects   customInspectorObjects;
    
    private Collider2D _coll;
    bool _isPanned = false;
    private void Start()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if (!CameraManager.Instance || !CameraManager.Instance.gameObject.activeInHierarchy) return;
        if (collision.GetComponentInParent<BallView>())
        {   
            if (customInspectorObjects.panCameraOnContact&& !_isPanned)
            {
                // pan the camera
                _isPanned = true;
				// CameraManager.Instance.PanCameraOnContact(customInspectorObjects.panDistance,customInspectorObjects.panTime,customInspectorObjects.panDirection,false);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!CameraManager.Instance || !CameraManager.Instance.gameObject.activeInHierarchy) return;
        if (collision.GetComponentInParent<BallView>())
        {   
            if (customInspectorObjects.panCameraOnContact && _isPanned)
            {            
                // pan the camera
                _isPanned = false;
                // CameraManager.Instance.PanCameraOnContact(customInspectorObjects.panDistance,customInspectorObjects.panTime,customInspectorObjects.panDirection,true);
            }
        }
    }
}

//非 MonoBehaviour 类，它不能直接在 Inspector 中显示，除非它被标记为 可序列化。
[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    public bool panCameraOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight; 

    [HideInInspector] public PanDirection panDirection;
    [HideInInspector] public float panDistance = 3.0f;
    [HideInInspector] public float panTime = 0.7f;
}
public enum PanDirection
{
    Up,
    Down,
    Left,
    Right
}

