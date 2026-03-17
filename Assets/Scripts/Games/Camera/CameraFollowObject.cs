using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _ballTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 1f;
    private Coroutine _turnCoroutine; 
    private bool _isFacingRight;
    private BallView _ballView;
    BallSim _ballSim;
    PlayerSim _carrier=null;
    public static CameraFollowObject Instance{get;private set;}
    
    private void Awake()
    {   
        Instance=this;
        _ballView = _ballTransform.gameObject.GetComponent<BallView>();
    }
    private void LateUpdate()
    {
        Vector2 pos;

        if (_carrier != null)
            pos = _carrier.Position;
        else
            pos = _ballSim.Position;

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    private bool FaceRight;
    public void CallTurn(bool faceRight) {
        if (FaceRight == faceRight) return;
        FaceRight = faceRight;
        if (_turnCoroutine != null)
            StopCoroutine(_turnCoroutine);

        _turnCoroutine = StartCoroutine(FlipYLerp());
    }
    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = FaceRight ? 0f : 180f;;
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
    public void FollowBall(BallSim ball)
    {
        _ballSim = ball;
        _carrier = null;
    }

    public void FollowCarrier(PlayerSim player)
    {
        _carrier = player;
    }
}
