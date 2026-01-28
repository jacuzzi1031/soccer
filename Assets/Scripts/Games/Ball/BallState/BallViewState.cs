
    using System;
    using UnityEngine;

    public class BallViewState {
        public const float GRAVITY = 8f;
        protected Animator animator; 
        protected Rigidbody2D rb;
        protected BallView BallView=null;
        public PlayerView carrier=null;
        protected ParticleSystem shotParticles;
        protected BallStateData stateData = new BallStateData();
        protected TriggerDetection playerDetectArea;
        public event Action<BallStateId, BallStateData> StateTransitionRequested;
        public Transform ballSprite;
        public const float BOUNCINESS = 0.8f;
        const float idleThreshold = 0.01f;
        public LayerMask mask;
        private RaycastHit2D[] hitBuffer = new RaycastHit2D[20];
        private ContactFilter2D filter;
        private CircleCollider2D colliderForWall;

        public virtual void Setup(BallView contextBallView,BallStateData ContextBallStateData,Animator ContextAnimator,ParticleSystem ContextParticles,TriggerDetection ContextplayerDetectArea,PlayerView ContextCarrier,Transform ContextBallSprite,Rigidbody2D ContextRigidBody,LayerMask ContextLayerMask,CircleCollider2D ContextColliderForWall) {
            animator = ContextAnimator;
            BallView = contextBallView;
            stateData = ContextBallStateData;
            shotParticles=ContextParticles;
            playerDetectArea=ContextplayerDetectArea;
            carrier = ContextCarrier;
            ballSprite=ContextBallSprite;
            rb= ContextRigidBody;
            mask=ContextLayerMask;
            colliderForWall=ContextColliderForWall;
        }
        public void TransitionState(BallStateId newBallStateId, BallStateData data = null)
        {
            StateTransitionRequested?.Invoke(newBallStateId, data ?? BallStateData.Build());
        }
        public void MoveVertical(float bounciness = 0.0f)
        {   
            //默认height不会是0，而是-0.888，导致一直削弱速度
            if (BallView.height > 0 || BallView.heightVelocity > 0) {
                float dt = Time.fixedDeltaTime;
                BallView.heightVelocity -= GRAVITY * dt;//heightVelocity 每帧位移
                BallView.height += BallView.heightVelocity;
                // Debug.Log("ball.height"+ball.height+" ball.heightVelocity:"+ball.heightVelocity);
                if (BallView.height < 0)
                {
                    BallView.height = 0;
                    if (BallView.heightVelocity < 0)
                    {
                        BallView.heightVelocity = -BallView.heightVelocity * bounciness;
                        BallView.velocity *= bounciness;
                    }
                }
            }
        }
        public void SetBallAnimationFromVelocity()
        {
            if (BallView.velocity.sqrMagnitude <= idleThreshold)
            {
                animator.Play("idle");
                animator.speed = 1;
                return;
            }

            if (BallView.velocity.x > 0)
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
            float friction = BallView.height > 0 ? BallView.frictionAir : BallView.frictionGround;
            BallView.velocity = Vector2.MoveTowards(BallView.velocity, Vector2.zero, friction* dt);
            Vector2 move = BallView.velocity * Time.fixedDeltaTime;
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
                BallView.velocity = Vector2.Reflect(BallView.velocity, hit.normal) * BOUNCINESS;
                BallView.SwitchViewState(BallStateId.FREEFORM);
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
