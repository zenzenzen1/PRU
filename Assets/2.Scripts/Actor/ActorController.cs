using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActorController : MonoBehaviour
{
    
    [Header("Collision")]

  
    [SerializeField] bool _useCollider = true;
    [SerializeField] float _skinWidth = 0.001f;
   
    [SerializeField] int _rayCountX = 3;
    [SerializeField] int _rayCountY = 2;        
   
    [SerializeField] Vector2 _colliderCenter = Vector2.zero;
    [SerializeField] Vector2 _colliderSize = Vector2.one;

    [Header("Gravity")]
    [SerializeField] bool _useGravity = true;  
    [SerializeField] float _gravityScale = 140f; 
    [SerializeField] float _maxFallSpeed = 40f; 

    float _raySpacingX, _raySpacingY; 
                                       
    float _velocityX, _velocityY;      
    float _movePosX, _movePosY;       
    float _slideVelocity, _slideDirection, _slideDeceleration;  

    float _deltaTime;                   

    bool _isRoofed, _isGrounded, _isLeftWalled, _isRightWalled;

    Vector2 _leftRayOrigin, _rightRayOrigin, _topRayOrigin, _bottomRayOrigin;   
    Vector2 _colliderSizeStore; 

    LayerMask _groundLayer;  

    Transform _actorTransform;

    
    public bool IsRoofed => _isRoofed;
    public bool IsGrounded => _isGrounded;
    public bool IsLeftWalled => _isLeftWalled;
    public bool IsRightWalled => _isRightWalled;
    public bool IsWalled => _isLeftWalled || _isRightWalled;

   
    public bool UseGravity
    {
        get
        {
            return _useGravity;
        }
        set
        {
            _useGravity = value;

            if (!value)
            {
                _velocityY = 0;
            }
        }
    }
    
  
    public float GravityScale { get => _gravityScale; set => _gravityScale = value; }
    public float MaxFallSpeed { get => _maxFallSpeed; set => _maxFallSpeed = value; }

 
    public float VelocityX { get => _velocityX; set => _velocityX = value; }
    public float VelocityY { get => _velocityY; set => _velocityY = value; }
    public bool IsSliding => _slideVelocity > 0;

    void Start()
    {
        _groundLayer = LayerMask.GetMask("Ground");
        _actorTransform = GetComponent<Transform>();

       
        if (_colliderSize.x <= 0 || _colliderSize.y <= 0)
        {
            _useCollider = _useGravity = false;
        }
    }

    void Update()
    {
      
        _deltaTime = Time.deltaTime;

       
        if (_useGravity)
        {
            HandleGravity();
        }

       
        _movePosX = (_velocityX + (_slideVelocity * _slideDirection)) * _deltaTime;
        _movePosY = _velocityY * _deltaTime;

       
        if (_useCollider)
        {
            ColliderUpdate();
            HandleCollision();
        }
      
        _actorTransform.Translate(new Vector2(_movePosX, _movePosY));

      
        if (_slideVelocity > 3.0f)
        {
            _slideVelocity -= _deltaTime * _slideDeceleration;
            _slideVelocity = Mathf.Clamp(_slideVelocity, 0f, float.MaxValue);
        }
        else
        {
            _slideVelocity = 0;
        }

       
        _velocityX = 0;
        _movePosX = _movePosY = 0;
    }

    
    void ColliderUpdate()
    {
       
        float centerX = _actorTransform.position.x + _colliderCenter.x;
        float centerY = _actorTransform.position.y + _colliderCenter.y;

      
        var colliderHalfSize = _colliderSize * 0.5f;

        
        var min = new Vector2(centerX - colliderHalfSize.x, centerY - colliderHalfSize.y);
        var max = new Vector2(centerX + colliderHalfSize.x, centerY + colliderHalfSize.y);

      
        _leftRayOrigin = new Vector2(min.x, min.y);
        _rightRayOrigin = new Vector2(max.x, min.y);
        _topRayOrigin = new Vector2(min.x, max.y);
        _bottomRayOrigin = new Vector2(min.x, min.y);
        
       
        if(_colliderSizeStore != _colliderSize)
        {
            _raySpacingX = _colliderSize.y / (_rayCountX - 1);
            _raySpacingY = _colliderSize.x / (_rayCountY - 1);
            _colliderSizeStore = _colliderSize;
        }
    }

    
    void HandleCollision()
    {
       
        if (_rayCountX > 0)
        {
            
            _isLeftWalled = _movePosX <= 0 && RayCollision(_rayCountX, -_movePosX, _raySpacingX, _leftRayOrigin, Vector2.left);
            _isRightWalled = _movePosX >= 0 && RayCollision(_rayCountX, _movePosX, _raySpacingX, _rightRayOrigin, Vector2.right);

            
            if ((_movePosX < 0 && _isLeftWalled) || (_movePosX > 0 && _isRightWalled))
            {
                _velocityX = 0;
            }
        }
      
        if(_rayCountY > 0)
        {
           
            _isRoofed = _movePosY >= 0 && RayCollision(_rayCountY, _movePosY, _raySpacingY, _topRayOrigin, Vector2.up);
            _isGrounded = _movePosY <= 0 && RayCollision(_rayCountY, -_movePosY, _raySpacingY, _bottomRayOrigin, Vector2.down);

           
            if((_movePosY > 0 && _isRoofed) || (_movePosY < 0 && _isGrounded))
            {
                _velocityY = 0;
            }
        }
    }

   
    /// <param name="rayCount"></param>
    /// <param name="rayLength"></param>
    /// <param name="raySpacing"></param>
    /// <param name="rayOrigin"></param>
    /// <param name="rayDirection"></param>
  
    bool RayCollision(int rayCount, float rayLength, float raySpacing, Vector2 rayOrigin, Vector2 rayDirection)
    {
        rayLength += _skinWidth + 0.01f;
        bool isHorizontalCollisionCheck = rayDirection.x != 0;

       
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 currentRayOrigin = rayOrigin;
            Vector2 rayOriginDir = isHorizontalCollisionCheck ? Vector2.up : Vector2.right;
            currentRayOrigin += rayOriginDir * (raySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(currentRayOrigin, rayDirection, rayLength, _groundLayer);
#if UNITY_EDITOR
            Debug.DrawRay(currentRayOrigin, rayDirection * rayLength * 3, Color.red);
#endif
            if(hit)
            {
                if(isHorizontalCollisionCheck)
                {
                    _movePosX = (hit.distance - _skinWidth) * rayDirection.x;
                }
                else
                {
                    _movePosY = (hit.distance - _skinWidth) * rayDirection.y;
                }
                return true;
            }
        }

       
        return false;
    }

    
    void HandleGravity()
    {
        if(_isGrounded) return;

        _velocityY -= _gravityScale * _deltaTime;
        _velocityY = Mathf.Clamp(_velocityY, -_maxFallSpeed, float.MaxValue);
    }

  
    /// <param name="velocity"></param>
    /// <param name="slideDirection"></param>
    /// <param name="slideDeceleration"></param>
    public void SlideMove(float velocity, float slideDirection, float slideDeceleration = 50f)
    {
        _slideVelocity = velocity;
        _slideDirection = slideDirection;
        _slideDeceleration = slideDeceleration;
    }

    
    public void SlideCancle()
    {
        _slideVelocity = 0;
    }


    void OnDrawGizmosSelected()
    {
        float offsetX = transform.position.x + _colliderCenter.x;
        float offsetY = transform.position.y + _colliderCenter.y;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(offsetX, offsetY), _colliderSize);
    }
}