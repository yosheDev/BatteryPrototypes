using UnityEngine;

public enum DamageTypes
{
    BluntForce,
    Explosives,
    Corrosion
}
namespace Magnet
{
    public interface IDamageable
    {
        void Damage(DamageTypes damageType);

        bool IsAffectedByDamageType(DamageTypes damageType);
    }
}
