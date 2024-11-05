using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingPiece : MonoBehaviour
{
    readonly int hashBroken = Animator.StringToHash("Broken");
    readonly int hashInit = Animator.StringToHash("Init");

    [SerializeField] float _collisionRange = 0.5f;  // 충돌 범위(radius)
    [SerializeField] bool _shouldDefinitelyDrop;    // 반드시 드롭되는 체력 회복 오브젝트인지 체크
    [SerializeField] AudioClip _pickUpSound;        // 플레이어가 해당 오브젝트와 접촉하여 체력을 회복했을때 재생되는 사운드

    LayerMask _playerLayer;

    Animator _anim;
    Transform _transform;
    PlayerDamage _playerDamage;
    ActorController _controller;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _transform = GetComponent<Transform>();
        _controller = GetComponent<ActorController>();
        _playerDamage = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDamage>();

        _playerLayer = LayerMask.GetMask("Player");
    }

    void OnEnable()
    {
        if (ShouldDropBasedOnProbability())
        {
          
            _controller.UseGravity = true;
            StartCoroutine(PlayerHealing());
        }
        else
        {
           
            ObjectPoolManager.instance.ReturnPoolObject(gameObject);
        }
    }

    
    /// <param name="healthPercent"></param>
    bool ShouldDropBasedOnProbability()
    {
        if(_shouldDefinitelyDrop) return true;

        float healthPercent = _playerDamage.GetHealthPercent();
        float dropRate;

       
        if (healthPercent > 0.75f)
        {
            dropRate = 0.15f;
        }
        else if (healthPercent > 0.5f)
        {
            dropRate = 0.375f;
        }
        else if (healthPercent > 0.25f)
        {
            dropRate = 0.6f;
        }
        else
        {
            dropRate = 0.85f;
        }

        return Random.value <= dropRate;
    }

    
    IEnumerator PlayerHealing()
    {
       
        _anim.SetTrigger(hashInit);
        float time = 0;
        float moveX = Random.Range(6.0f, 18.0f);
        float direction = Mathf.Sign(Random.Range(-1f, 1f));
        float velocityX = moveX * direction;
        _controller.VelocityY = Random.Range(6f, 14f);
        bool isGrounded = false;

       
        while(time < 15.0f)
        {
            time += Time.deltaTime;
            if (!isGrounded)
            {
               
                _controller.VelocityX = velocityX;
                if (_controller.IsGrounded)
                {
                    isGrounded = true;
                    _controller.VelocityX = 0f;
                    _controller.SlideMove(moveX, direction);
                }
            }

           
            bool isLeftWalled = _controller.VelocityX < 0 && _controller.IsLeftWalled;
            bool isRightWalled = _controller.VelocityX > 0 && _controller.IsRightWalled;
            if (isLeftWalled || isRightWalled)
            {
                direction = -direction;
                _controller.VelocityX = moveX * direction;
            }

         
            if(Physics2D.OverlapCircle(_transform.position, _collisionRange, _playerLayer))
            {
                _playerDamage.HealthRecovery(1);
                _anim.SetTrigger(hashBroken);
                _controller.UseGravity = false;
                SoundManager.instance.SoundEffectPlay(_pickUpSound);

                yield return YieldInstructionCache.WaitForSeconds(1.0f);
                break;
            }

            yield return null;
        }
       
        ObjectPoolManager.instance.ReturnPoolObject(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _collisionRange);
    }
}
