using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Actor : MonoBehaviour
{
    [SerializeField] private List<PoolObjectData> _poolObjectDataList;

    protected float deltaTime;    
    protected bool isDead;         

    protected Transform actorTransform;
    protected ActorController controller = null;

    protected Animator animator;
    protected Dictionary<string, int> animationHash = new Dictionary<string, int>();

    protected bool FacingRight { get; private set; }

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        actorTransform = GetComponent<Transform>();

        if (TryGetComponent(out ActorController controller))
        {
            this.controller = controller;
        }

        
        foreach (var poolObject in _poolObjectDataList)
        {
            ObjectPoolManager.instance.CreatePool(poolObject);
        }

        
        FacingRight = actorTransform.localScale.x > 0;
    }

    #region Animator

    
    /// <param name="name"></param>
   
    protected int GetAnimationHash(string name)
    {
     
        if (!animationHash.TryGetValue(name, out int hash))
        {
            animationHash.Add(name, Animator.StringToHash(name));
            hash = Animator.StringToHash(name);
        }

        return hash;
    }

    
    protected float GetAnimatorNormalizedTime() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

   
    protected bool IsAnimationEnded() => GetAnimatorNormalizedTime() >= 0.99f;

    
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
   
    protected bool IsAnimatorNormalizedTimeInBetween(float minTime, float maxTime)
    {
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return normalizedTime >= minTime && normalizedTime <= maxTime;
    }

    #endregion

    #region Actor Flip

    
    protected void Flip()
    {
        FacingRight = !FacingRight;
 
        int newScaleX = FacingRight ? 1 : -1;
        actorTransform.localScale = new Vector2(newScaleX, 1);
    }

   
    /// <param name="direction"></param>
    protected void SetFacingDirection(float direction)
    {
      
        if (direction != 0)
        {
            actorTransform.localScale = new Vector2(Mathf.Sign(direction), 1);
            FacingRight = actorTransform.localScale.x > 0;
        }
    }

    #endregion
}
