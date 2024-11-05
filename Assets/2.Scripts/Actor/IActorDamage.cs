using System.Collections;


public interface IActorDamage
{
    public delegate void KnockbackEventHandler(KnockBack knockBack);   
    public delegate IEnumerator DiedEventHandler();                    

    public int CurrentHealth {get; set;} 
    public float GetHealthPercent();       

    public void TakeDamage(int damage, KnockBack knockBack);   
}