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
    private Ball ball;

    public static CameraFollowObject Instance{get;private set;}
    
    private void Awake()
    {   
        Instance=this;
        ball = _ballTransform.gameObject.GetComponent<Ball>();
    }
    private void Update()
    {   
        
        if (ball.carrier) {
            _isFacingRight = ball.carrier.headingRight;
            transform.position = ball.carrier.transform.position;
        }
        else {
            transform.position = _ballTransform.position;
        }
    }
    public void CallTurn()
    {
        //Tracked Object Offset由于是乘以目标的 Transform 的旋转矩阵，所以也会反过来
        _turnCoroutine = StartCoroutine(FlipYLerp() );
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
