using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _ballTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;
    private Coroutine _turnCoroutine; 
    private bool _isFacingRight;
    private BallView _ballView;

    public static CameraFollowObject Instance{get;private set;}
    
    private void Awake()
    {   
        Instance=this;
        _ballView = _ballTransform.gameObject.GetComponent<BallView>();
    }
    private void Update()
    {   
        
        if (_ballView.carrier) {
            _isFacingRight = _ballView.carrier.playerSim.HeadingRight;
            transform.position = _ballView.carrier.transform.position;
        }
        else {
            transform.position = _ballTransform.position;
        }
    }
    public void CallTurn()
    {
        if (_turnCoroutine != null)
            StopCoroutine(_turnCoroutine);

        _turnCoroutine = StartCoroutine(FlipYLerp());
    }
    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;
    
        float elapsedTime = 0f;
        while (elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime; 
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, elapsedTime/_flipYRotationTime);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(0f, endRotationAmount, 0f);
        _turnCoroutine = null;
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight; 

        if (_isFacingRight)
        {
            return 0f;
        } 
        else
        {
            return 180f;
        }
    }
}
