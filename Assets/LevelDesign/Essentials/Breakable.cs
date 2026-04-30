using UnityEngine;
using Magnet;

public class Breakable : MonoBehaviour, IDamageable
{
    public void Damage(DamageTypes damageType)
    {
        Destroy(gameObject);
    }

    public bool IsAffectedByDamageType(DamageTypes damageType)
    {
        if (damageType == DamageTypes.Explosives)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
