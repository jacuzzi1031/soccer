
    using System;
    using UnityEngine;

    public class BallState {
        public const float GRAVITY = 8f;
        protected Animator animator; 
        protected Rigidbody2D rb;
        protected Ball ball=null;
        public Player carrier=null;
        protected ParticleSystem shotParticles;
        protected BallStateData stateData = new BallStateData();
        protected TriggerDetection playerDetectArea;
        public event Action<Ball.State, BallStateData> StateTransitionRequested;
        public Transform ballSprite;
        public const float BOUNCINESS = 0.8f;
        const float idleThreshold = 0.01f;
        public LayerMask mask;
        private RaycastHit2D[] hitBuffer = new RaycastHit2D[20];
        private ContactFilter2D filter;
        private CircleCollider2D colliderForWall;

        public virtual void Setup(Ball contextBall,BallStateData ContextBallStateData,Animator ContextAnimator,ParticleSystem ContextParticles,TriggerDetection ContextplayerDetectArea,Player ContextCarrier,Transform ContextBallSprite,Rigidbody2D ContextRigidBody,LayerMask ContextLayerMask,CircleCollider2D ContextColliderForWall) {
            animator = ContextAnimator;
            ball = contextBall;
            stateData = ContextBallStateData;
            shotParticles=ContextParticles;
            playerDetectArea=ContextplayerDetectArea;
            carrier = ContextCarrier;
            ballSprite=ContextBallSprite;
            rb= ContextRigidBody;
            mask=ContextLayerMask;
            colliderForWall=ContextColliderForWall;
        }
        public void TransitionState(Ball.State newState, BallStateData data = null)
        {
            StateTransitionRequested?.Invoke(newState, data ?? BallStateData.Build());
        }
        public void MoveVertical(float bounciness = 0.0f)
        {   
            //默认height不会是0，而是-0.888，导致一直削弱速度
            if (ball.height > 0 || ball.heightVelocity > 0) {
                float dt = Time.fixedDeltaTime;
                ball.heightVelocity -= GRAVITY * dt;//heightVelocity 每帧位移
                ball.height += ball.heightVelocity;
                // Debug.Log("ball.height"+ball.height+" ball.heightVelocity:"+ball.heightVelocity);
                if (ball.height < 0)
                {
                    ball.height = 0;
                    if (ball.heightVelocity < 0)
                    {
                        ball.heightVelocity = -ball.heightVelocity * bounciness;
                        ball.velocity *= bounciness;
                    }
                }
            }
        }
        public void SetBallAnimationFromVelocity()
        {
            if (ball.velocity.sqrMagnitude <= idleThreshold)
            {
                animator.Play("idle");
                animator.speed = 1;
                return;
            }

            if (ball.velocity.x > 0)
            {
                animator.Play("roll");
                animator.speed = 1;
            }
            else
            {
                animator.Play("rollback");
                animator.speed = 1;
            }
        }
        public virtual void _Update() {
        }
        public virtual void _FixedUpdate() {
        
        }





        public virtual void OnExit() {
        
        }

        public virtual bool CanAirInteract() {
            return false;
        }
        public virtual void OnEnter() {
            filter = new ContactFilter2D();
            filter.SetLayerMask(mask);
            filter.useLayerMask = true;
            filter.useTriggers = false;
            
        }
        
        // string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
        // Debug.Log($"Hit {hit.collider.name}, Layer: {layerName}, Tag: {hit.collider.tag}");
        public void MoveHorizontal()
        {
            float dt = Time.fixedDeltaTime;
            float friction = ball.height > 0 ? ball.frictionAir : ball.frictionGround;
            ball.velocity = Vector2.MoveTowards(ball.velocity, Vector2.zero, friction* dt);
            Vector2 move = ball.velocity * Time.fixedDeltaTime;
            if (move.sqrMagnitude < 0.001f)
                return;

            Vector2 dir = move.normalized;
            
            float radius = colliderForWall.radius;
            
            RaycastHit2D hit = Physics2D.CircleCast(
                rb.position,
                radius,
                dir,
                move.magnitude + colliderForWall.radius,
                mask 
            );
            if (hit.collider != null)
            {
                Vector2 contactPos =rb.position + dir * hit.distance;
                rb.MovePosition(contactPos);
                ball.velocity = Vector2.Reflect(ball.velocity, hit.normal) * BOUNCINESS;
                ball.SwitchState(Ball.State.FREEFORM);
                SoundManager.Instance.Play(SoundManager.Instance.audioRefs.BOUNCE);
            }
            else
            {
                rb.MovePosition(rb.position + move);
            }
        }


        public virtual bool CanCarriedBall() {
            return false;
        }
    }
