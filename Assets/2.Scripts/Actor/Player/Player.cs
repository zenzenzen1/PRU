using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(ActorController), typeof(PlayerDamage))]

public class Player : Actor
{
 
    [SerializeField] float _moveSpeed = 16.5f;                  
    [SerializeField] float _groundMoveAcceleration = 9.865f;   
    [SerializeField] float _groundMoveDeceleration = 19f;    
    [SerializeField] float _airMoveAcceleration = 5.5f;    
    [SerializeField] float _airMoveDeceleration = 8f;   
    [SerializeField] float _runDustEffectDelay = 0.2f;  

  
    [Space(10)]
    [SerializeField] float _jumpForce = 24f;           
    [SerializeField] float _jumpHeight = 4.5f;         
    [SerializeField] float _doubleJumpHeight = 2.0f;  
    [SerializeField] float _maxCoyoteTime = 0.06f;      
    [SerializeField] float _maxJumpBuffer = 0.25f;  

   
    [Space(10)]
    [SerializeField] float _slidingForce = 24f;         
    [SerializeField] float _dodgeDuration = 0.25f;       
    [SerializeField] float _dodgeInvinsibleTime = 0.15f; 
    [SerializeField] float _dodgeCooldown = 0.15f;      
    [SerializeField] float _dodgeDustEffectDelay = 0.1f; 

  
    [Space(10)]
    [SerializeField] float _wallSlidingSpeed = 8.5f; 
    [SerializeField] float _wallJumpHeight = 1.5f;  
    [SerializeField] float _wallJumpXForce = 8f;    
    [SerializeField] float _wallSlideDustEffectDelay = 0.15f;

  
    [Space(10)]
    [SerializeField] int _power = 3;                           
    [SerializeField] float _basicAttackKnockBackForce = 8.0f;  
    [SerializeField] float _fireBallKnockBackForce = 7.5f;     

   
    [Space(10)]
    [SerializeField] Image _healingEffect;  

    
    [Space(10)]
    [SerializeField] AudioClip _swingSound;        
    [SerializeField] AudioClip _fireBallSound;     
    [SerializeField] AudioClip _attackHitSound;    
    [SerializeField] AudioClip _stepSound;          
    [SerializeField] AudioClip _jumpSound;          

    float _coyoteTime;     
    float _maxJumpHeight;

    float _moveX;   

    float _nextWallSlideDustEffectTime = 0;
    float _nextRunDustEffectTime = 0;      

    bool _canMove = true;       
    bool _canWallSliding = true; 
    bool _hasDoubleJumped;       
    bool _canDodge = true;      

  

  
    int _xAxis;            
    bool _leftMoveInput;    
    bool _rightMoveInput;   
    float _timeHeldBackInput;      

   
    bool _jumpInput;       
    bool _jumpDownInput;   
    float _jumpBuffer;     
   
    bool _attackInput;     
    bool _specialInput;   
    bool _dodgeInput;       
    bool _isJumped;           
    bool _isFalling;           
    bool _isAttacking;       
    bool _isDodging;          
    bool _isWallSliding;      
    bool _isResting;           
    bool isBeingKnockedBack;   

    KnockBack _basicAttackKnockBack = new KnockBack();
    KnockBack _fireBallKnockBack = new KnockBack();

    Transform _backCliffChecked;           
    Coroutine _knockedBackCoroutine = null; 
    PlayerAttack _attack;                  
    PlayerDrivingForce _drivingForce;       
    PlayerDamage _damage;                 

    #region MonoBehaviour

    protected override void Awake()
    {
        base.Awake();

       
        _backCliffChecked = transform.Find("BackCliffChecked").GetComponent<Transform>();
        _damage = GetComponent<PlayerDamage>();
        _drivingForce = GetComponent<PlayerDrivingForce>();
        _attack = GetComponent<PlayerAttack>();

      
        _basicAttackKnockBack.force = _basicAttackKnockBackForce;
        _fireBallKnockBack.force = _fireBallKnockBackForce;

  
        _damage.KnockBack += OnKnockedBack;
        _damage.Died += OnDied;
    }

    void Start()
    {
        if (!GameManager.instance.IsStarted())
        {
            
            actorTransform.position = GameManager.instance.playerStartPos;
            actorTransform.localScale = new Vector3(GameManager.instance.playerStartlocalScaleX, 1, 1);

            _damage.CurrentHealth = GameManager.instance.playerCurrentHealth;
            _drivingForce.CurrentDrivingForce = GameManager.instance.playerCurrentDrivingForce;

            GameManager.instance.GameSave();
        }
        else
        {
         
            GameManager.instance.playerCurrentHealth = _damage.CurrentHealth;
            GameManager.instance.playerCurrentDrivingForce = _drivingForce.CurrentDrivingForce;

            if (GameManager.instance.firstStart)
            {
             
                GameManager.instance.playerResurrectionPos = actorTransform.position;
                GameManager.instance.resurrectionScene = SceneManager.GetActiveScene().name;
                GameManager.instance.firstStart = false;
            }
            else
            {
                if (GameManager.instance.resurrectionScene == SceneManager.GetActiveScene().name)
                {
                    actorTransform.position = GameManager.instance.playerStartPos;
                }
            }
        }
    }

    void Update()
    {
       
        if (isDead || _isResting) return;

      
        deltaTime = Time.deltaTime;

       
        HandleInput();

   
        HandleMove();
        HandleJump();
        HandleWallSlide();
        HandleFallingAndLanding();
        HandleAttack();
        HandleDodge();

       
        AnimationUpdate();
    }

    #endregion


    #region Input

    
    void HandleInput()
    {
      
        if (GameManager.instance.currentGameState != GameManager.GameState.Play) return;

       
        _leftMoveInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.MoveLeft);
        _rightMoveInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.MoveRight);
        _xAxis = _leftMoveInput ? -1 : _rightMoveInput ? 1 : 0;

       
        _jumpInput = GameInputManager.PlayerInput(GameInputManager.PlayerActions.Jump);
        _jumpDownInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Jump);

       
        _attackInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Attack);
        _specialInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.SpecialAttack);
        _dodgeInput = GameInputManager.PlayerInputDown(GameInputManager.PlayerActions.Dodge);
    }

    #endregion

    #region Move

    
    void HandleMove()
    {
      
        if (!_canMove) return;

        if(_xAxis != 0)
        {
          
            _moveX += (controller.IsGrounded ? _groundMoveAcceleration : _airMoveAcceleration) * deltaTime;

            if (actorTransform.localScale.x != _xAxis)
            {
                if(!_isAttacking)
                {
                    Flip();
                    _moveX = 0;
                    if(controller.IsGrounded)
                    {
                        _nextRunDustEffectTime = 0;
                    }
                }
            }

         
            if(controller.IsGrounded)
            {
               _nextRunDustEffectTime -= deltaTime;
               if(_nextRunDustEffectTime <= 0)
               {
                 
                    ObjectPoolManager.instance.GetPoolObject("RunDust", actorTransform.position, actorTransform.localScale.x);
                    _nextRunDustEffectTime = _runDustEffectDelay;   
                   
                    SoundManager.instance.SoundEffectPlay(_stepSound);
                    GamepadVibrationManager.instance.GamepadRumbleStart(0.02f, 0.017f);
               }
            }
        }
        else
        {
           
            _moveX -= (controller.IsGrounded ? _groundMoveDeceleration : _airMoveDeceleration) * deltaTime;
            _nextRunDustEffectTime = 0;
        }
        _moveX = Mathf.Clamp(_moveX, 0f, 1f);  
        controller.VelocityX = _xAxis * _moveSpeed * _moveX;   
    }

    #endregion

    #region Jump

    
    void HandleJump()
    {
       
        if(controller.IsGrounded || controller.IsWalled)
        {
            _hasDoubleJumped = false;
        }

       
        if (!_isJumped)
        {
         
            if(JumpInputBuffer() && CoyoteTime())
            {
             
                if (!_isAttacking && !_isDodging && !isBeingKnockedBack)
                {
                    StartJump();
                }
            }
          
            else if(PlayerLearnedSkills.hasLearnedDoubleJump)
            {
             
                if(!_hasDoubleJumped && !controller.IsGrounded && !_isWallSliding)
                {
                    if(_jumpDownInput)
                    {
                        StartDoubleJump();
                    }
                }
            }
        }
        else
        {
            ContinueJumping();
        }
    }

   
    void StartJump()
    {
        _isJumped = true;  

       
        _coyoteTime = _maxCoyoteTime;
        _jumpBuffer = _maxJumpBuffer;

     
        _maxJumpHeight = actorTransform.position.y + _jumpHeight;

       
        ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
        GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);
        SoundManager.instance.SoundEffectPlay(_stepSound);
        SoundManager.instance.SoundEffectPlay(_jumpSound);
    }

    
    void StartDoubleJump()
    {
       
        _isJumped = true;
        _hasDoubleJumped = true;
       
        _maxJumpHeight = actorTransform.position.y + _doubleJumpHeight;
       
        animator.SetTrigger(GetAnimationHash("DoubleJump"));
       
        ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
        GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);
        SoundManager.instance.SoundEffectPlay(_jumpSound);
    }

    
    void ContinueJumping()
    {
        controller.VelocityY = _jumpForce;
     
        if (_maxJumpHeight <= actorTransform.position.y)
        {
            _isJumped = false;
            controller.VelocityY = _jumpForce * 0.75f;
        }
      
        else if (!_jumpInput || controller.IsRoofed)
        {
            _isJumped = false;
            controller.VelocityY = _jumpForce * 0.5f;
        }
    }

   
    bool JumpInputBuffer()
    {
        if (_jumpInput)
        {
            
            if (_jumpBuffer < _maxJumpBuffer)
            {
                _jumpBuffer += deltaTime;
                return true;
            }
        }
        else
        {
            
            _jumpBuffer = 0;
        }
        return false;
    }

  
    bool CoyoteTime()
    {
        if(controller.IsGrounded)
        {
          
            _coyoteTime = 0;
            return true;
        }
        else if(_coyoteTime < _maxCoyoteTime)
        {
           
            _coyoteTime += deltaTime;
            return true;
        }
        
        return false;
    }

  
    void HandleFallingAndLanding()
    {
        if (controller.VelocityY < -5f)
        {
           
            _isFalling = true;
        }
        else if (controller.IsGrounded && _isFalling)
        {
           
            _isFalling = false;
          
            SoundManager.instance.SoundEffectPlay(_stepSound);
            ObjectPoolManager.instance.GetPoolObject("JumpDust", actorTransform.position);
            GamepadVibrationManager.instance.GamepadRumbleStart(0.25f, 0.05f);
        }
    }

    #endregion

    #region ClimbingWall

    void HandleWallSlide()
    {
        if(!PlayerLearnedSkills.hasLearnedClimbingWall || !_canWallSliding || _isAttacking) return;

        if(!_isWallSliding)
        {
            if(controller.IsWalled && !controller.IsGrounded && controller.VelocityY < 0)
            {
                int wallDirection = controller.IsLeftWalled ? -1 : 1;
                if (_xAxis == wallDirection)
                {
                    StartWallSliding();
                }
            }
        }
        else
        {
            ContinueWallSliding();
        }
    }

    void StartWallSliding()
    {
        _isWallSliding = true;
        _isDodging = _canMove = false;
        controller.SlideCancle();
    }

    void ContinueWallSliding()
    {
        int wallDirection = controller.IsLeftWalled ? -1 : 1;
        float dustEffectXPos = actorTransform.position.x + (wallDirection == 1 ? 0.625f : -0.625f);
        Vector2 dustEffectPos = new Vector2(dustEffectXPos, actorTransform.position.y + 1.25f);

        controller.VelocityY = Mathf.Clamp(controller.VelocityY, -_wallSlidingSpeed, float.MaxValue);

        bool backInput = (wallDirection == 1 && _leftMoveInput) || 
                         (wallDirection == -1 && _rightMoveInput);
        if (backInput)
        {
            _timeHeldBackInput += deltaTime;
            if(_timeHeldBackInput >= 0.2f)
            {
                WallSlidingCancle();
            }
        }
        else
        {
            _timeHeldBackInput = 0;
        }

        if(_jumpDownInput)
        {
            _isJumped =true;
            _coyoteTime = _maxCoyoteTime;
            _maxJumpHeight = actorTransform.position.y + _wallJumpHeight;

            Flip();
            controller.SlideMove(_wallJumpXForce, -wallDirection, 30f);
            _moveX = 1.0f; 

            ObjectPoolManager.instance.GetPoolObject("WallJumpDust", dustEffectPos, -actorTransform.localScale.x);
            GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);
            SoundManager.instance.SoundEffectPlay(_stepSound);

            WallSlidingCancle();
        }

        if(!controller.IsWalled || controller.IsGrounded || isBeingKnockedBack)
        {
            WallSlidingCancle();
        }

        _nextWallSlideDustEffectTime -= deltaTime;
        if (_nextWallSlideDustEffectTime <= 0)
        {
            _nextWallSlideDustEffectTime = _wallSlideDustEffectDelay;
            ObjectPoolManager.instance.GetPoolObject("WallSlideDust", dustEffectPos,actorTransform.localScale.x);
            GamepadVibrationManager.instance.GamepadRumbleStart(0.1f, 0.033f);
            SoundManager.instance.SoundEffectPlay(_swingSound);
        }
    }

    void WallSlidingCancle()
    {
        _isWallSliding = false;
        _canMove = true;
        _timeHeldBackInput = 0;
        _nextWallSlideDustEffectTime = _wallSlideDustEffectDelay;
    }

    #endregion

    #region Attack

    void HandleAttack()
    {
        if(_isAttacking || _isWallSliding || _isDodging || isBeingKnockedBack) return;

        if(_attackInput)
        {
            StartCoroutine(BasicAttack());
        }
        else if(_specialInput)
        {
            if (_drivingForce.TryConsumeDrivingForce(1))
            {
                StartCoroutine(FireBall());
            }
        }
    }

    IEnumerator BasicAttack()
    {
        _isAttacking = true;

        bool isHit = false;
        bool isNextAttacked = false;

        animator.SetTrigger(GetAnimationHash("BasicAttack"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));

        _basicAttackKnockBack.direction = actorTransform.localScale.x;

        if (controller.IsGrounded)
        {
            controller.SlideMove(_moveSpeed, actorTransform.localScale.x, 65f);
        }

        // 1프레임 이후 실행
        yield return null;
        
        SoundManager.instance.SoundEffectPlay(_swingSound);

        while(!IsAnimationEnded())
        {
            if(isBeingKnockedBack) break;

            if(!isHit)
            {
                // 공격이 적중하지 않았을 때 지속적으로 공격을 실행하고 적중했는지 체크
                if (_attack.IsAttacking(_power, _basicAttackKnockBack))
                {

                    for (int i = 0; i < _attack.HitCount; i++)
                    {
                        _drivingForce.IncreaseDrivingForce();
                    }
                    SoundManager.instance.SoundEffectPlay(_attackHitSound);
                    GamepadVibrationManager.instance.GamepadRumbleStart(0.5f, 0.05f);

                    controller.SlideMove(11.5f, -actorTransform.localScale.x);

                    if (!controller.IsGrounded)
                    {
                        controller.VelocityY = 15;
                    }

                    isHit = true;
                }
            }

            if(!Physics2D.Raycast(_backCliffChecked.position, Vector2.down, 1.0f, LayerMask.GetMask("Ground")))
            {
                controller.SlideCancle();
            }
            
            _canMove = !controller.IsGrounded;

            if (IsAnimatorNormalizedTimeInBetween(0.4f, 0.87f))
            {
                if (_attackInput)
                {
                    isNextAttacked = true;
                }
            }
            else if(IsAnimatorNormalizedTimeInBetween(0.87f, 1.0f))
            {
                if(isNextAttacked)
                {
                    if(_xAxis == actorTransform.localScale.x)
                    {
                        controller.SlideMove(_moveSpeed, actorTransform.localScale.x, 65f);
                    }
                    animator.SetTrigger(GetAnimationHash("NextAttack"));
                    // 사운드 재생
                    SoundManager.instance.SoundEffectPlay(_swingSound);

                    isHit = isNextAttacked = false;
                }
            }

            yield return null;
        }

        AttackEnd();
    }

    IEnumerator FireBall()
    {
        _isAttacking = true;

        bool hasSpawnedFireBall = false;

        animator.SetTrigger(GetAnimationHash("FireBall"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));
        _fireBallKnockBack.direction = actorTransform.localScale.x;

        yield return null;

        SoundManager.instance.SoundEffectPlay(_fireBallSound);

        while(!IsAnimationEnded())
        {
            if(isBeingKnockedBack) break;

            if(controller.IsGrounded)
            {
                _moveX = 0;
                _canMove = false;
            }

            if(IsAnimatorNormalizedTimeInBetween(0.28f, 0.4f))
            {
                if(!hasSpawnedFireBall)
                {
                    float scaleX = actorTransform.localScale.x;
                    float addPosX = actorTransform.position.x + (0.66f * scaleX);
                    float addPosY = actorTransform.position.y + 1.12f;
                    Vector2 addPos = new Vector2(addPosX, addPosY);
                    float angle = scaleX == 1 ? 180 : 0;

                    ObjectPoolManager.instance.GetPoolObject("FireBall", addPos, scaleX, angle);

                    hasSpawnedFireBall = true;  
                }
            }

            yield return null;
        }

        AttackEnd();
    }

    void AttackEnd()
    {
        animator.SetTrigger(GetAnimationHash("AnimationEnd"));
        _isAttacking = false;
        _canMove = true;
    }

    #endregion

    #region Dodge

    void HandleDodge()
    {
        if (_isAttacking || _isWallSliding || _isDodging || isBeingKnockedBack) return;

        if(_dodgeInput && _canDodge && controller.IsGrounded)
        {
            StartCoroutine(Dodging());
        }
    }

    IEnumerator Dodging()
    {
        float nextDodgeDustEffectTime = 0;
        float dodgeInvinsibleTime = _dodgeInvinsibleTime;
        float dodgeDuration = _dodgeDuration;

        // 회피 중인 상태로 설정
        _damage.IsDodged = _isDodging = true;
        _canDodge = _canMove = false;

        animator.SetTrigger(GetAnimationHash("Sliding"));
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));

        // 1프레임 대기
        yield return null;
        
        controller.SlideMove(_slidingForce, actorTransform.localScale.x);

        while(dodgeDuration > 0)
        {
            if(isBeingKnockedBack) break;

            if(nextDodgeDustEffectTime <= 0)
            {
                ObjectPoolManager.instance.GetPoolObject("RunDust",
                                                        actorTransform.position,
                                                        actorTransform.localScale.x);
                nextDodgeDustEffectTime = _dodgeDustEffectDelay;
                SoundManager.instance.SoundEffectPlay(_jumpSound);
            }
            else
            {
                nextDodgeDustEffectTime -= deltaTime;
            }

            if(dodgeInvinsibleTime <= 0)
            {
                _damage.IsDodged = false;
            }
            else
            {
                dodgeInvinsibleTime -= deltaTime;
            }

            dodgeDuration -= deltaTime;
            yield return null;
        }

        DodgeEnd();
    }

    void DodgeEnd()
    {
        controller.SlideCancle();
        animator.SetTrigger(GetAnimationHash("AnimationEnd"));

        _isDodging = false;
        _canMove = true;

        StartCoroutine(DodgeCooldown());
    }

    IEnumerator DodgeCooldown()
    {
        yield return YieldInstructionCache.WaitForSeconds(_dodgeCooldown);
        _canDodge = true;
    }

    #endregion

    #region Rest

    public void RestAtCheckPoint(Vector2 checkPointPos)
    {
        if (!controller.IsGrounded || _isResting) return;

        GameManager.instance.playerResurrectionPos = checkPointPos;
        GameManager.instance.resurrectionScene = SceneManager.GetActiveScene().name;

        DeadEnemyManager.ClearDeadEnemies();

        GameManager.instance.GameSave();

        StartCoroutine("Resting");
    }

    IEnumerator Resting()
    {
        float red = _healingEffect.color.r;
        float green = _healingEffect.color.g;
        float blue = _healingEffect.color.b;
        float alpha = 0.5f;

        _isResting = true;
        animator.ResetTrigger(GetAnimationHash("AnimationEnd"));
        animator.SetTrigger(GetAnimationHash("Rest"));
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.2f);

        _healingEffect.enabled = AccessibilitySettingsManager.screenFlashes;
        _healingEffect.color = new Color(red, green, blue, alpha);
        _damage.HealthRecovery(_damage.maxHealth);
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.1f);

        while (alpha > 0f)
        {
            alpha -= 0.025f;
            _healingEffect.color = new Color(red, green, blue, alpha);

            yield return YieldInstructionCache.WaitForSecondsRealtime(0.01f);
        }
        yield return YieldInstructionCache.WaitForSecondsRealtime(0.1f);

        animator.SetTrigger(GetAnimationHash("AnimationEnd"));
        _isResting = false;
    }

    #endregion

    #region Damage

    void OnKnockedBack(KnockBack knockBack)
    {
        controller.UseGravity = true;

        actorTransform.Translate(new Vector2(0, 0.1f));

        controller.SlideMove(knockBack.force, knockBack.direction);
        _canMove = false;

        StopCoroutine("Resting");
        _isResting = false;

        _isAttacking = _isDodging = _isWallSliding = _isJumped = false;
        isBeingKnockedBack = true;
        GamepadVibrationManager.instance.GamepadRumbleStart(0.8f, 0.068f);

        if(!isDead)
        {
            controller.VelocityY = 24f;
            if(_knockedBackCoroutine != null)
            {
                StopCoroutine(_knockedBackCoroutine);
                _knockedBackCoroutine = null;
            }
            _knockedBackCoroutine = StartCoroutine(KnockedBackCoroutine());
        }
    }

    IEnumerator KnockedBackCoroutine()
    {
        animator.SetTrigger(GetAnimationHash("KnockBack"));

        while(controller.IsSliding)
        {
            yield return null;
        }

        _canMove = true;
        isBeingKnockedBack = false;

        animator.SetTrigger(GetAnimationHash("KnockBackEnd"));

        _knockedBackCoroutine = null;
    }

    IEnumerator OnDied()
    {
        isDead = true;
        GameManager.instance.HandlePlayerDeath();
        animator.SetTrigger(GetAnimationHash("Die"));
        ScreenEffect.instance.BulletTimeStart(0.3f, 1.0f);
        yield return YieldInstructionCache.WaitForSecondsRealtime(2.5f);
        GameManager.instance.playerCurrentHealth = _damage.maxHealth;
        GameManager.instance.playerCurrentDrivingForce = 0;
        SceneTransition.instance.LoadScene(GameManager.instance.resurrectionScene);
    }

    #endregion

    #region Animator

    void AnimationUpdate()
    {
        animator.SetFloat(GetAnimationHash("MoveX"), _moveX);
        animator.SetFloat(GetAnimationHash("FallSpeed"), controller.VelocityY);

        animator.SetBool(GetAnimationHash("IsGrounded"), controller.IsGrounded);
        animator.SetBool(GetAnimationHash("IsWallSliding"), _isWallSliding);
    }

    #endregion
}