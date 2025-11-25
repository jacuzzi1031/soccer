using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;
    [SerializeField] private Ball ball;

    [Header("Controls for Screen Y during player up/down")]
    [SerializeField] private float _downScreenYAmount = 0.3f;
    [SerializeField] private float _upScreenYAmount = 0.65f;
    [SerializeField] private float _tranScreenYTime = 1.00f; 
 
    

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;
    private Coroutine _swapOffsetCameraCoroutine;

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;
    private float _normScreenYAmount;
    [HideInInspector] public bool IsLerpingScreenY = false; 
    [HideInInspector] public bool isLerpOffsetXAction = false; 
    private float _currentTargetScreenY;
    private Vector2 _carriedTrackedObjectOffset=new Vector2(1.0f,0f);
    private Vector2 _freeformTrackedObjectOffset=new Vector2(0f,0f);
    private Vector2 _currentTrackedObjectOffset;
    private Vector2 _startingTrackedObjectOffset;
    private float _tranOffsetTime = 1.0f;
    private float _startingOrthographicSize;
    private Coroutine _PowerShotZoomCoroutine;
    private LensSettings currentCameraMLens;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 

        for (int i = 0; i < _allVirtualCameras.Length; i++)
        {
            if (_allVirtualCameras[i].enabled) 
            {
                //set the current active camera
                _currentCamera = _allVirtualCameras[i];

                //set the framing transposer
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        _normScreenYAmount=_framingTransposer.m_ScreenY;


        _startingTrackedObjectOffset = _freeformTrackedObjectOffset;
        //set the Lens.OrthographicSize
        _startingOrthographicSize = _currentCamera.m_Lens.OrthographicSize;
    }

    public void OnEnable() {
        //每次都是currentState会new一个新的 所以不能ball.currentState.OnBallFreeformAction+=
        ball.OnBallFreeformAction+= CurrentStateOnOnBallFreeformAction;
    }


    private void CurrentStateOnOnBallFreeformAction(object sender, bool e) {
        _startingTrackedObjectOffset=e?_freeformTrackedObjectOffset:_carriedTrackedObjectOffset;
        LerpOffsetX(e);
        
    }
    #region Lerp offsetX

    public void LerpOffsetX(bool isFreeform)
    {
        _currentTrackedObjectOffset = isFreeform ?
            _freeformTrackedObjectOffset :
            _carriedTrackedObjectOffset;
        if (Mathf.Approximately(_framingTransposer.m_TrackedObjectOffset.x,
                _currentTrackedObjectOffset.x))
            return;
        
        if (isLerpOffsetXAction)
        {
            StopCoroutine(_swapOffsetCameraCoroutine);
        }

        isLerpOffsetXAction = true;
        _swapOffsetCameraCoroutine = StartCoroutine(LerpOffsetXAction());

    }

    private IEnumerator LerpOffsetXAction()
    {
        float startOffset = _framingTransposer.m_TrackedObjectOffset.x;
        float endOffset = _currentTrackedObjectOffset.x;

        float elapsedTime = 0f;

        while (elapsedTime < _tranOffsetTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _tranOffsetTime;
            float lerped = Mathf.Lerp(startOffset, endOffset, t);

            var offset = _framingTransposer.m_TrackedObjectOffset; 
            offset.x = lerped;
            _framingTransposer.m_TrackedObjectOffset = offset;

            yield return null;
        }
        
        var finalOffset = _framingTransposer.m_TrackedObjectOffset;
        finalOffset.x = endOffset;
        _framingTransposer.m_TrackedObjectOffset = finalOffset;

        isLerpOffsetXAction = false;
    }
    #endregion


    #region Lerp the Y Screen

    public void LerpScreenY(float velocityY)
    {
        // 更新目标 Y 值，基于玩家的速度
        if (velocityY > 0) // 上升
        {
            _currentTargetScreenY = _upScreenYAmount;
        }
        else if (velocityY < 0) // 下降
        {
            _currentTargetScreenY = _downScreenYAmount;
        }
        else // 速度接近 0
        {
            _currentTargetScreenY = _normScreenYAmount;
        }

        if (Mathf.Approximately(_currentTargetScreenY, _framingTransposer.m_ScreenY))
            return;

        // 如果当前没有在 lerp，就启动一个新的协程
        if (!IsLerpingScreenY)
        {
            IsLerpingScreenY = true;
            _lerpYPanCoroutine = StartCoroutine(LerpYAction());
        }
    }

    private IEnumerator LerpYAction()
    {
        float startScreenY = _framingTransposer.m_ScreenY;
        float endScreenY = _currentTargetScreenY;

        // 进行平滑过渡
        float elapsedTime = 0f;
        while (elapsedTime < _tranScreenYTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedPanAmount = Mathf.Lerp(startScreenY, endScreenY, elapsedTime / _tranScreenYTime);
            _framingTransposer.m_ScreenY = lerpedPanAmount;
            yield return null;
        }

        // 最终设置目标位置
        _framingTransposer.m_ScreenY = endScreenY;

        // 协程结束，重置 lerp 标记
        IsLerpingScreenY = false;
    }
    #endregion
    
    #region Pan Camera
    
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;
        
        //inter
        if (!panToStartingPos)
        {
            //set the direction and distance
            switch (panDirection)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break; 
                case PanDirection.Left:
                    endPos = Vector2.right;//+
                    break;
                case PanDirection.Right:
                    endPos = Vector2.left;//-
                    break;
                default:
                    break;
            }

            endPos *= panDistance;

            startingPos = _startingTrackedObjectOffset;
            endPos += startingPos;
        }
        // handle the direction settings when moving back to the starting position
        else
        {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        // handle the actual panning of the camera
        float elapsedTime = 0f;
        while(elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            _framingTransposer.m_TrackedObjectOffset = panLerp;
            yield return null;
        }
    }
    #endregion

    #region PowerShot

    public void PowerShotZoom(float zoomSize,float zoomTime,bool isShot) {
        if (_PowerShotZoomCoroutine != null)
            StopCoroutine(_PowerShotZoomCoroutine);
        _PowerShotZoomCoroutine = StartCoroutine(ZoomAction( zoomSize, zoomTime, isShot));
    }

    private IEnumerator ZoomAction(float zoomSize, float zoomTime, bool isShot) {
        float startingSize;
        float endSize;
        if (isShot) {
            startingSize=_currentCamera.m_Lens.OrthographicSize;
            endSize=zoomSize;
        }
        else {
            startingSize = _currentCamera.m_Lens.OrthographicSize;
            endSize = _startingOrthographicSize;
        }
        float elapsedTime = 0f;
        while(elapsedTime < zoomTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomTime;
            var lens = _currentCamera.m_Lens;
            lens.OrthographicSize = Mathf.Lerp(startingSize, endSize, t);
            _currentCamera.m_Lens = lens;
            yield return null;
        }
    }

    #endregion
    
    #region Swap Cameras
    
    public void SwapCamera(CinemachineVirtualCamera cameraForBall, CinemachineVirtualCamera cameraForPlayer,bool isFreeform)
    {
        if (!isFreeform)
        {
            cameraForPlayer.enabled = true;
            cameraForBall.enabled = false;
            _currentCamera = cameraForPlayer;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if (isFreeform)
        {
            cameraForBall.enabled = true; 
            cameraForPlayer.enabled = false;
            _currentCamera = cameraForBall;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        } 
    }
    public void SwapCameraCrossWall(CinemachineVirtualCamera cameraFromLeft, CinemachineVirtualCamera cameraFromRight, Vector2 triggerExitDirection)
    {
        if (_currentCamera == cameraFromLeft && triggerExitDirection.x > 0f)
        {
            cameraFromRight.enabled = true;
            cameraFromLeft.enabled = false;
            _currentCamera = cameraFromRight;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if (_currentCamera == cameraFromRight && triggerExitDirection.x < 0f)
        {
            cameraFromLeft.enabled = true; 
            cameraFromRight.enabled = false;
            _currentCamera = cameraFromRight;
            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        } 
    }
    #endregion
}
