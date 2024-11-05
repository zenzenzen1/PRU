using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDamage : MonoBehaviour, IActorDamage
{
    
    public IActorDamage.KnockbackEventHandler KnockBack = null;
  
    public IActorDamage.DiedEventHandler Died;

    SpriteRenderer _spriteRenderer; 
    Material _defalutMaterial;     

    public int CurrentHealth { get; set; }     
    public int MaxHealth { get; set; }       
    public bool SuperArmor { get; set; }      
    public bool IsDead { get; protected set; }   
    public bool IsInvincibled { get; set; }     
    public Material BlinkMaterial { get; set; }

   
    public float GetHealthPercent() =>  CurrentHealth / (float)MaxHealth;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defalutMaterial = _spriteRenderer.material;

        if (KnockBack == null)
        {
            SuperArmor = true;
        }

        CurrentHealth = MaxHealth;
    }

    
    /// <param name="damage"></param>
    /// <param name="knockBack"></param>
    public virtual void TakeDamage(int damage, KnockBack knockBack)
    {
       
        if(IsDead) return;

      
        if(!SuperArmor)
        {
            KnockBack(knockBack);
        }

      
        CurrentHealth -= damage;
        if(CurrentHealth <= 0)
        {
            StartCoroutine(Died());
            IsDead = true;
            return;
        }

       
        StartCoroutine(BlinkEffect());
    }

    
    IEnumerator BlinkEffect()
    {
        _spriteRenderer.material = BlinkMaterial;

        yield return YieldInstructionCache.WaitForSeconds(0.1f);

        _spriteRenderer.material = _defalutMaterial;
    }
}
