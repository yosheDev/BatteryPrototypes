using UnityEngine;
using System.Collections.Generic;
using FunctionLibrary;

public class ChargeParticleCollision : ParticleCollision
{
    public Vector2 particleSizeRange = new Vector2(.1f, .5f);
    public Vector2 chargeAmountRange = new Vector2(1f, 5f);
    protected override void AssignTriggerColliders() /// Override this event in children to assign different colliders.
    {
        Collider2D playerCol = FindAnyObjectByType<BatteryController>().gameObject.GetComponent<Collider2D>();
        _particleSystem.trigger.AddCollider(playerCol);
    }

    protected override void ParticleCollected(Component otherCollider, float particleSize)
    {
        if (otherCollider.gameObject.GetComponent<Battery>() != null)
        {
            otherCollider.gameObject.GetComponent<Battery>().AddPercent(FunctionLibraryF.MapRangeClamped(0.1f, 0.5f, 1f, 5f, particleSize));
        }
    }
}
