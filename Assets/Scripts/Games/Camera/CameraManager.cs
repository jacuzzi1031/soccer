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
    [SerializeField] private float _tranScreenYTime = 0.5f; 
 
    

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;
    private Coroutine _swapOffsetCameraCoroutine;

    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;
    private float _normScreenYAmount;
    [HideInInspector] public bool IsLerpingScreenY = false; 
    [HideInInspector] public bool isLerpOffsetXAction = false; 
    private float _currentTargetScreenY;
    private Vector2 _carriedTrackedObjectOffset=new Vector2(24.0f,0f);
    private Vector2 _freeformTrackedObjectOffset=new Vector2(0f,0f);
    private Vector2 _currentTrackedObjectOffset;
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

        
        //set the Lens.OrthographicSize
        _startingOrthographicSize = _currentCamera.m_Lens.OrthographicSize;
        
        GameInterface.Interface.EventSystem.Subscribe<BallFreeformToLerpCameraOffsetEvent>(CurrentStateOnOnBallFreeformAction);
    }

    public void OnDisable() {
        GameInterface.Interface.EventSystem.Unsubscribe<BallFreeformToLerpCameraOffsetEvent>(CurrentStateOnOnBallFreeformAction);
    }


    private void CurrentStateOnOnBallFreeformAction(BallFreeformToLerpCameraOffsetEvent e) {
        _currentTrackedObjectOffset = e.IsFreeform ?
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
    #region Lerp offsetX
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

    #region Lerp the Y Screen

    public void LerpScreenY(float velocityY)
    {
        float target = _normScreenYAmount;

        if (velocityY > 0)
            target = _upScreenYAmount;
        else if (velocityY < 0)
            target = _downScreenYAmount;

        if (Mathf.Approximately(target, _currentTargetScreenY))
            return;

        _currentTargetScreenY = target;

        if (_lerpYPanCoroutine != null)
            StopCoroutine(_lerpYPanCoroutine);

        _lerpYPanCoroutine = StartCoroutine(LerpYAction());
    }

    IEnumerator LerpYAction()
    {
        float start = _framingTransposer.m_ScreenY;
        float time = 0;

        while (time < _tranScreenYTime)
        {
            time += Time.deltaTime;

            _framingTransposer.m_ScreenY =
                Mathf.Lerp(start, _currentTargetScreenY, time / _tranScreenYTime);

            yield return null;
        }

        _framingTransposer.m_ScreenY = _currentTargetScreenY;
    }
    #endregion

}
