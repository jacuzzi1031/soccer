using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;
    [FormerlySerializedAs("ball")] [SerializeField] private BallView ballView;

    [Header("Controls for Screen Y during player up/down")]
    [SerializeField] private float _downScreenYAmount = 0.4f;
    [SerializeField] private float _upScreenYAmount = 0.8f;
    [SerializeField] private float _tranScreenYTime = 0.4f; 
 
    

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;
    private Coroutine _swapOffsetCameraCoroutine;

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;
    private float _normScreenYAmount;
    [HideInInspector] public bool IsLerpingScreenY = false; 
    [HideInInspector] public bool isLerpOffsetXAction = false; 
    private float _currentTargetScreenY;
    private Vector2 _carriedTrackedObjectOffset=new Vector2(18.0f,0f);
    private Vector2 _freeformTrackedObjectOffset=new Vector2(0f,0f);
    private Vector2 _currentTrackedObjectOffset;
    private Vector2 _startingTrackedObjectOffset;
    [SerializeField]private float _CarryOrFreeBallTranOffsetTime = 0.5f;
    private float _startingOrthographicSize;
    private Coroutine _PowerShotZoomCoroutine;
    private LensSettings currentCameraMLens;
    public float ShrinkZoomSizeWhileShoot = 80f;
    public float ShrinkZoomSizeTime = 1.0f;
    
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


        _startingTrackedObjectOffset = _carriedTrackedObjectOffset;
        //set the Lens.OrthographicSize
        _startingOrthographicSize = _currentCamera.m_Lens.OrthographicSize;
        
        GameInterface.Interface.EventSystem.Subscribe<BallFreeformToLerpCameraOffsetEvent>(CurrentStateOnOnBallFreeformAction);
    }


    private void CurrentStateOnOnBallFreeformAction(BallFreeformToLerpCameraOffsetEvent e) {
        _startingTrackedObjectOffset=e.IsFreeform?_freeformTrackedObjectOffset:_carriedTrackedObjectOffset;
        LerpOffsetX(e.IsFreeform);
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

        while (elapsedTime < _CarryOrFreeBallTranOffsetTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / _CarryOrFreeBallTranOffsetTime;
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
        if (velocityY > 0)
            _currentTargetScreenY = _upScreenYAmount;
        else if (velocityY < 0)
            _currentTargetScreenY = _downScreenYAmount;
        else
            _currentTargetScreenY = _normScreenYAmount;
        if (Mathf.Approximately(_currentTargetScreenY, _framingTransposer.m_ScreenY))
            return;
        if (_lerpYPanCoroutine != null)
            StopCoroutine(_lerpYPanCoroutine);
        _lerpYPanCoroutine = StartCoroutine(LerpYAction());
    }

    private IEnumerator LerpYAction()
    {
        while (!Mathf.Approximately(_framingTransposer.m_ScreenY, _currentTargetScreenY))
        {
            _framingTransposer.m_ScreenY = Mathf.Lerp(
                _framingTransposer.m_ScreenY,
                _currentTargetScreenY,
                Time.deltaTime * (1f / _tranScreenYTime)
            );

            yield return null;
        }

        _framingTransposer.m_ScreenY = _currentTargetScreenY;
    }
    #endregion
    
    #region Pan Camera
    
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos)
    {   
        if (_panCameraCoroutine != null)
            StopCoroutine(_panCameraCoroutine);
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }
    private void OnDisable()
    {   
        StopAllCoroutines();
        GameInterface.Interface.EventSystem
            .Unsubscribe<BallFreeformToLerpCameraOffsetEvent>(
                CurrentStateOnOnBallFreeformAction
            );
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
        _framingTransposer.m_TrackedObjectOffset = endPos;
        _panCameraCoroutine = null;
    }
    #endregion

    #region PowerShot

    public void PowerShotZoom(bool isShot) {
        if (_PowerShotZoomCoroutine != null)
            StopCoroutine(_PowerShotZoomCoroutine);
        _PowerShotZoomCoroutine = StartCoroutine(ZoomAction( isShot));
    }

    private IEnumerator ZoomAction( bool isShot)
    {
        float startingSize = _currentCamera.m_Lens.OrthographicSize;
        float endSize = isShot ? ShrinkZoomSizeWhileShoot : _startingOrthographicSize;

        float elapsedTime = 0f;

        while (elapsedTime < ShrinkZoomSizeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / ShrinkZoomSizeTime;

            var lens = _currentCamera.m_Lens;
            lens.OrthographicSize = Mathf.Lerp(startingSize, endSize, t);
            _currentCamera.m_Lens = lens;

            yield return null;
        }
        var finalLens = _currentCamera.m_Lens;
        finalLens.OrthographicSize = endSize;
        _currentCamera.m_Lens = finalLens;
    }

    #endregion
    
    #region Swap Cameras withoutUse
    
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
