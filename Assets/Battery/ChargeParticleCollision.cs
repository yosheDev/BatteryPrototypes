using UnityEngine;
using System.Collections.Generic;

public class ChargeParticleCollision : ParticleCollision
{
    public byte chargeAmount = 2;
    protected override void AssignTriggerColliders() /// Override this event in children to assign different colliders.
    {
        Collider2D playerCol = FindFirstObjectByType<BatteryController>().gameObject.GetComponent<Collider2D>();
        _particleSystem.trigger.AddCollider(playerCol);
    }

    protected override void ParticleCollected(Component otherCollider)
    {
        if (otherCollider.gameObject.GetComponent<Battery>() != null)
        {
            otherCollider.gameObject.GetComponent<Battery>().AddPercent(chargeAmount);
        }
    }
}
