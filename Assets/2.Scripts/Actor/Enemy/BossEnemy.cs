using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class DelegateDrone
{
    public GameObject gameObject;
    public SpriteRenderer spriteRenderer;
}


[RequireComponent(typeof(BossEnemyDamage))]
public class BossEnemy : Enemy
{
    public GameObject bossHealthBarUI;                   
    public GameObject[] invisibleWall;                     
    public List<string> attackName = new List<string>();   
    public DelegateDrone[] delegateDrones;                 
    public Sprite[] droneSprite;                            
    public SpriteRenderer eyeSprite;       

   
    public Transform topLeftMovePos;
    public Transform topRightMovePos;
    public Transform centerMovePos;
    public Transform bottomLeftMovePos;
    public Transform bottomRightMovePos;

    public AudioClip battleMusic;          
    public AudioClip targetMissileSound;    
    public AudioClip radiationBulletSound;  

    int _phase = 1;           
    int _currentAttackIndex;    
    float _attackDelay;        

    bool _isDetected;     
    bool _isAttacking;      
    bool _isBattleStarted; 

    Transform _playerTransform;
    Transform _currentMovePos;
    List<Transform> movePos = new List<Transform>();

    Coroutine _attackCoroutine = null;
    AudioClip _prevMusic;
    SpriteRenderer _sprite;
    BossEnemyDamage _bossEnemyDamage;

    protected override void Awake()
    {
        // if (DeadEnemyManager.IsDeadBoss(keyName))
        // {
        //     isDead = true;
        //     Destroy(gameObject);
        //     return;
        // }

        base.Awake();

        
        var movePosList = new List<Transform>() { topRightMovePos, topRightMovePos, centerMovePos, bottomLeftMovePos, bottomRightMovePos };
        movePos.AddRange(movePosList);

       
        _currentMovePos = centerMovePos;

       
        _sprite = transform.GetComponent<SpriteRenderer>();
        _sprite.color = new Color32(255, 255, 255, 0);

      
        _attackDelay = enemyData.attackDelay;

        
        _bossEnemyDamage = (BossEnemyDamage)enemyDamage;
        _bossEnemyDamage.IsInvincibled = true;
        _bossEnemyDamage.KnockBack = null;
        _bossEnemyDamage.Died = null;
        _bossEnemyDamage.phaseChangedEvent += OnPhaseChanged;
        _bossEnemyDamage.KnockBack += OnKnockedBack;
        _bossEnemyDamage.Died += OnDied;

        _playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    void Update()
    {
        
        if (isDead) return;

      
        deltaTime = Time.deltaTime;

        if (!_isDetected)
        {
            
            float distance = (_playerTransform.position - actorTransform.position).sqrMagnitude;
            if (distance < Mathf.Pow(enemyData.detectRange, 2))
            {
                StartCoroutine(BattleStart());
            }
        }
        else if (_isBattleStarted)
        {
          
            if (!_isAttacking)
            {
                _attackDelay -= deltaTime;

                if (_attackDelay <= 0)
                {
                    StartAttack();
                }
            }
        }
    }

    
    IEnumerator BattleStart()
    {
        _isDetected = true;

       
        bossHealthBarUI.SetActive(true);

       
        foreach (var obj in invisibleWall)
        {
            obj.SetActive(true);
        }

        
        _prevMusic = SoundManager.instance.GetCurrentMusic();
        SoundManager.instance.MusicPlay(battleMusic);

       
        while (actorTransform.position != centerMovePos.position)
        {
            actorTransform.position = Vector3.MoveTowards(actorTransform.position, centerMovePos.position, 4f * deltaTime);
            yield return null;
        }
        yield return YieldInstructionCache.WaitForSeconds(0.5f);

      
        _sprite.color = new Color32(255, 255, 255, 255);
        animator.SetTrigger(GetAnimationHash("Reappear"));

        yield return null;
        float time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);

        _isBattleStarted = true;
        _bossEnemyDamage.IsInvincibled = false;
    }


  
    void StartAttack()
    {
        _currentAttackIndex++;
        if (_currentAttackIndex == attackName.Count)
        {
            _currentAttackIndex = 0;
        }
        string nextAttack = attackName[_currentAttackIndex];
        if (nextAttack != null)
        {
            _isAttacking = true;
            _attackCoroutine = StartCoroutine(nextAttack);
        }
    }

    
    IEnumerator Move()
    {
     
        _bossEnemyDamage.IsInvincibled = true;

       
        animator.SetTrigger(GetAnimationHash("Disappear"));
        yield return null;

  
        float time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);

      
        List<Transform> nextMovePos = movePos.ToList();
        nextMovePos.Remove(_currentMovePos);
        int movePosIndex = Random.Range(0, nextMovePos.Count);
        
       
        while(actorTransform.position != nextMovePos[movePosIndex].position)
        {
            actorTransform.position = Vector3.MoveTowards(actorTransform.position, nextMovePos[movePosIndex].position, enemyData.patrolSpeed * deltaTime);
            yield return null;
        }
        _currentMovePos = nextMovePos[movePosIndex];

      
        yield return YieldInstructionCache.WaitForSeconds(0.05f);
        animator.SetTrigger(GetAnimationHash("Reappear"));
        yield return null;
        time = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return YieldInstructionCache.WaitForSeconds(time);
        
       
        _bossEnemyDamage.IsInvincibled = false;
        _isAttacking = false;
        _attackDelay = enemyData.attackDelay;
    }

    
    IEnumerator DroneAttack()
    {
    
        int repeatCount = Random.Range(2, 4);

        for (int i = 0; i < repeatCount; i++)
        {
            
            int currentPhase = _phase;
           
            Coroutine[] droneAttackCoroutine = new Coroutine[currentPhase];

          
            yield return StartCoroutine(DroneSetup(currentPhase));
            yield return YieldInstructionCache.WaitForSeconds(0.5f);

          
            for (int j = 0; j < currentPhase; j++)
            {
                if (delegateDrones[j].spriteRenderer.sprite == droneSprite[0])
                {
                    droneAttackCoroutine[j] = StartCoroutine(TargetMissile(delegateDrones[j].gameObject.transform));
                }
                else if (delegateDrones[j].spriteRenderer.sprite == droneSprite[1])
                {
                    droneAttackCoroutine[j] = StartCoroutine(RadiationBulletFast(delegateDrones[j].gameObject.transform));
                }
                else
                {
                    droneAttackCoroutine[j] = StartCoroutine(RadiationBulletSlow(delegateDrones[j].gameObject.transform));
                }
            }

         
            yield return YieldInstructionCache.WaitForSeconds(2f);
            for (int j = 0; j < droneAttackCoroutine.Length; j++)
            {
                if(droneAttackCoroutine[j] != null)
                    StopCoroutine(droneAttackCoroutine[j]);
            }

         
            yield return YieldInstructionCache.WaitForSeconds(0.5f);
            yield return StartCoroutine(DroneReturn(currentPhase));

          
            yield return null;
            for (int j = 0; j < delegateDrones.Length; j++)
            {
                delegateDrones[j].gameObject.transform.position = new Vector3(-1000, -1000, 0);
            }
        }

     
        _attackDelay = enemyData.attackDelay;
        _isAttacking = false;
    }

    
    /// <param name="phase"></param>
   
    IEnumerator DroneSetup(int phase)
    {
      
        const float minX = 5;
        const float maxX = 12;

        List<Transform> dronsTransform = new List<Transform>();
        List<Vector3> movePos = new List<Vector3>();

       
        for (int i = 0; i < phase; i++)
        {
          
            Vector3 pos = actorTransform.position;
            pos.z += 1;
            delegateDrones[i].gameObject.transform.position = pos;
            delegateDrones[i].spriteRenderer.sprite = droneSprite[Random.Range(0, droneSprite.Length)];
            dronsTransform.Add(delegateDrones[i].gameObject.transform);

            
            Vector3 randomPos;
            if (_currentMovePos == topLeftMovePos || _currentMovePos == bottomLeftMovePos)
            {
                randomPos.x = Random.Range(actorTransform.position.x + minX, actorTransform.position.x + maxX);
            }
            else if (_currentMovePos == topRightMovePos || _currentMovePos == bottomRightMovePos)
            {
                randomPos.x = Random.Range(actorTransform.position.x - minX, actorTransform.position.x - maxX);
            }
            else
            {
                randomPos.x = Random.Range(actorTransform.position.x - maxX,actorTransform.position.x + maxX);
                if (randomPos.x < actorTransform.position.x && randomPos.x > actorTransform.position.x - minX)
                {
                    randomPos.x = actorTransform.position.x - minX;
                }
                else if (randomPos.x >= actorTransform.position.x && randomPos.x < actorTransform.position.x + minX)
                {
                    randomPos.x = actorTransform.position.x + minX;
                }
            }
            randomPos.y = Random.Range(actorTransform.position.y - 1.5f,
                                       actorTransform.position.y + 4);
           
            if (randomPos.y < actorTransform.position.y && randomPos.y > actorTransform.position.y - 2)
            {
                randomPos.y = actorTransform.position.y - 2;
            }
            else if (randomPos.y >= actorTransform.position.y && randomPos.y < actorTransform.position.y + 2)
            {
                randomPos.y = actorTransform.position.y + 2;
            }
            randomPos.z = actorTransform.position.z + 1;

           
            if(i > 0)
            {
                if(randomPos.x < movePos[0].x && randomPos.x > movePos[0].x - 3)
                {
                    randomPos.x = movePos[0].x - 3;
                }
                else if (randomPos.x >= movePos[0].x && randomPos.x < movePos[0].x + 3)
                {
                    randomPos.x = movePos[0].x + 3;
                }

                if (randomPos.y < movePos[0].y && randomPos.y > movePos[0].y - 3)
                {
                    randomPos.y = movePos[0].y - 3;
                }
                else if (randomPos.y >= movePos[0].y && randomPos.y < movePos[0].y + 3)
                {
                    randomPos.y = movePos[0].y + 3;
                }
            }

            movePos.Add(randomPos);
        }

       
        bool[] droneArrival = new bool[_phase];
        while(true)
        {
            for(int i = 0; i < dronsTransform.Count; i++)
            {
                if(dronsTransform[i].position != movePos[i])
                {
                    dronsTransform[i].position = Vector3.MoveTowards(dronsTransform[i].position, 
                                                            movePos[i], 
                                                            19.0f * deltaTime);
                }
                else droneArrival[i] = true;
            }

            if(droneArrival.Length == 1)
            {
                if (droneArrival[0] == true) yield break;
            }
            else
            {
                if (droneArrival[0] == true || droneArrival[1] == true) 
                {
                    yield break;
                }
            }

            yield return null;
        }
    }

    
    /// <param name="droneTransform"></param>
    IEnumerator TargetMissile(Transform droneTransform)
    {
        while (!isDead)
        {
            
            Vector2 firePos = droneTransform.position;
            float x = firePos.x - _playerTransform.position.x;
            float y = firePos.y - _playerTransform.position.y;
            float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

            ObjectPoolManager.instance.GetPoolObject("TargetMissile", droneTransform.position, 1, angle);
            SoundManager.instance.SoundEffectPlay(targetMissileSound);

           
            yield return YieldInstructionCache.WaitForSeconds(0.375f);
        }
    }

    
    /// <param name="droneTransform"></param>
    IEnumerator RadiationBulletFast(Transform droneTransform)
    {
       
        Vector2 firePos = droneTransform.position;
        float x = firePos.x - _playerTransform.position.x;
        float y = firePos.y - _playerTransform.position.y;
        float currentAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        float time = 0;
        const float Delay = 0.175f;
        while (!isDead)
        {
           
            for (int i = 0; i < 4; i++)
            {
                float bulletAngle = currentAngle + (90 * i);
                ObjectPoolManager.instance.GetPoolObject("RadiationBulletFast", droneTransform.position, 1, bulletAngle);
            }
          
            SoundManager.instance.SoundEffectPlay(radiationBulletSound);
            
           
            if (time >= 0.6f)
            {
                currentAngle += 10f;
            }
            else time += Delay;

           
            yield return YieldInstructionCache.WaitForSeconds(Delay);
        }
    }

   
    /// <param name="droneTransform"></param>
    IEnumerator RadiationBulletSlow(Transform droneTransform)
    {
        
        Vector2 firePos = droneTransform.position;
        float x = firePos.x - _playerTransform.position.x;
        float y = firePos.y - _playerTransform.position.y;
        float currentAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

        while (!isDead)
        { 
            for (int i = 0; i < 4; i++)
            {
                float bulletAngle = currentAngle + (90 * i);
                ObjectPoolManager.instance.GetPoolObject("RadiationBulletSlow", droneTransform.position, 1, bulletAngle);
            }
           
            SoundManager.instance.SoundEffectPlay(radiationBulletSound);
            
            currentAngle += 30f;
          
            yield return YieldInstructionCache.WaitForSeconds(0.3f);
        }
    }

  
    IEnumerator DroneReturn(int phase)
    {
        List<Transform> dronsTransform = new List<Transform>();
        for (int i = 0; i < phase; i++)
        {
            dronsTransform.Add(delegateDrones[i].gameObject.transform);
        }

      
        Vector3 returnTarget = actorTransform.position;
        returnTarget.z = dronsTransform[0].position.z;

       
        bool[] droneArrival = new bool[phase];
        while (true)
        {
            for (int i = 0; i < dronsTransform.Count; i++)
            {
                if (dronsTransform[i].position != returnTarget)
                {
                    dronsTransform[i].position = Vector3.MoveTowards(dronsTransform[i].position, returnTarget, 24f * deltaTime);
                }
                else
                {
                    droneArrival[i] = true;
                }
            }

            if (droneArrival.Length == 1)
            {
                if (droneArrival[0] == true) yield break;
            }
            else
            {
                if (droneArrival[0] == true || droneArrival[1] == true)
                {
                    yield break;
                }
            }

            yield return null;
        } 
    }

    
    protected override IEnumerator OnDied()
    {
        isDead = true;

       
        // StopCoroutine(_attackCoroutine);
        animator.SetTrigger(GetAnimationHash("Idle"));

       
        ScreenEffect.instance.ShakeEffectStart(0.15f, 0.5f);
        ScreenEffect.instance.BulletTimeStart(0f, 1.0f);

       
        foreach(var delegateDrone in delegateDrones)
        {
            delegateDrone.gameObject.transform.position = new Vector2(-1000, -1000);
        }

    
        yield return null;

       
        Vector2 position = new Vector2(transform.position.x, transform.position.y + 2);
        for (int i = 0; i < 2; i++)
        {
            ObjectPoolManager.instance.GetPoolObject("HealingPieceDefinitelyDrop", position);
        }

       
        DeadEnemyManager.AddDeadBoss(keyName);

        
        bossHealthBarUI.SetActive(false);

       
        SoundManager.instance.MusicPlay(_prevMusic);

    
        while (eyeSprite.color.a > 0)
        {
            eyeSprite.color = new Color(255, 255, 255, eyeSprite.color.a - 0.1f);
            yield return YieldInstructionCache.WaitForSeconds(0.1f);
        }

      
        yield return YieldInstructionCache.WaitForSeconds(1.0f);

  
        float fallingDuration = 3.0f;
        float fallSpeed = 2.0f;
        while (fallingDuration > 0)
        {
            actorTransform.Translate(fallSpeed * Vector2.down * deltaTime);
            fallSpeed += deltaTime * 7;
            fallingDuration -= deltaTime;
            yield return null;
        }

    
        foreach (var obj in invisibleWall)
        {
            obj.SetActive(false);
        }

        Destroy(gameObject);
    }

   
    void OnPhaseChanged()
    {
      
        _phase++;

       
        float playerDamage = GameObject.Find("Player").GetComponent<PlayerDamage>().GetHealthPercent();
        int healingPieceCount = playerDamage <= 0.5f ? 2 :
                                playerDamage < 1f ? 1 : 0;
        for (int i = 0; i < healingPieceCount; i++)
        {
            Vector2 position = new Vector2(transform.position.x, transform.position.y + 2);
            ObjectPoolManager.instance.GetPoolObject("HealingPieceDefinitelyDrop", position);
        }
    }
}