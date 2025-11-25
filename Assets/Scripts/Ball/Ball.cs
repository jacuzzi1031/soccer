using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{   
    public  float BOUNCINESS = 0.8f;
    private const float DISTANCE_HIGH_PASS = 5.0f;
    private const int DURATION_TUMBLE_LOCK = 200;
    private const float DURATION_PASS_LOCK = 500f;
    private const float KICKOFF_PASS_DISTANCE = 1.66f;
    private const float TUMBLE_HEIGHT_VELOCITY = 0.16f;
    public Vector2 velocity=Vector2.zero;
    private float castDistance = 3.33f;
    public float height=0.0f;
    public Player carrier;
    public float heightVelocity=0.0f;
    [SerializeField] public LayerMask collideForScoreAreaLayer;
    
    
    [SerializeField] private TriggerDetection playerDetectArea;
    [SerializeField] private TriggerDetection playerProximityArea;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem shotParticles;
    [SerializeField] private Transform ballSprite;
    [SerializeField]  private Rigidbody2D rb;
    public enum State {CARRIED, FREEFORM, SHOT}
    public BallStateFactory ballStateFactory=new BallStateFactory();
    [HideInInspector]public BallState currentState;
    public LayerMask LayerMask;
    public CircleCollider2D  colliderForWall;
    

    public float frictionAir = 1.94f;
    public float frictionGround = 13.8f;
    
    public event EventHandler<bool> OnBallFreeformAction;
    private void Awake() {

        SwitchState(State.FREEFORM);
    }
    private void Update()
    {
        ballSprite.localPosition = Vector3.up * height;
        currentState?._Update();
    }
    private void FixedUpdate()
    {
        currentState?._FixedUpdate();
    }
    
    public void SwitchState(State type, BallStateData data = null)
    {
        if (currentState != null)
        {
            currentState.StateTransitionRequested -= SwitchState;
            currentState.OnExit();
        }
        currentState = ballStateFactory.GetFreshState(type);
        currentState.Setup(this, data ?? new BallStateData(),animator,shotParticles,playerDetectArea,carrier,ballSprite,rb,LayerMask,colliderForWall);
        currentState.StateTransitionRequested += SwitchState;
        currentState.OnEnter();
    }
    bool IsHeadedForScoringArea(Collider2D scoringArea)
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, velocity.normalized, castDistance, collideForScoreAreaLayer);

        if (hit.collider == null)
            return false;

        return hit.collider == scoringArea;
    }
    public void Tumble(Vector2 tumbleVelocity)
    {
        velocity = tumbleVelocity;
        carrier = null;
        heightVelocity = TUMBLE_HEIGHT_VELOCITY;

        // 切换状态
        SwitchState(State.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }
    public void Stop() {
        velocity = Vector2.zero;
    }

    public bool CanAirInteract() {
        return currentState != null && currentState.CanAirInteract();
    }

    public void shoot(Vector2 ShotVelocity) {
        velocity = ShotVelocity;
        carrier = null;
        SwitchState(Ball.State.SHOT);
    }
    


    public void TriggerBallFreeform(bool value)
    {
        OnBallFreeformAction?.Invoke(this, value);
    }
}
