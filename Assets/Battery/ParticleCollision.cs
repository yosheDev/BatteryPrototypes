using UnityEngine;
using System.Collections.Generic;

public class ParticleCollision : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    List<ParticleSystem.Particle> collectedParticles = new List<ParticleSystem.Particle>();

    private void Start()
    {
        AssignTriggerColliders();
    }

    protected virtual void AssignTriggerColliders() /// Override this event in children to assign different colliders.
    {
        Collider2D playerCol = FindFirstObjectByType<BatteryController>().gameObject.GetComponent<Collider2D>();
        _particleSystem.trigger.AddCollider(playerCol);
    }

    private void OnParticleTrigger()
    {
        int triggeredParticles = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, collectedParticles, out var colliderData);

        for (int i = 0; i < triggeredParticles; i++)
        {
            ParticleSystem.Particle p = collectedParticles[i];
            p.remainingLifetime = 0;
            collectedParticles[i] = p;

            if (colliderData.GetCollider(i, 0) != null)
            {
                ParticleCollected(colliderData.GetCollider(i, 0));
            }
            else
            {
                ParticleCollected(null);
            }
        }

        _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, collectedParticles);
    }

    protected virtual void ParticleCollected(Component otherCollider) /// Override this event in children.
    {

    }
}
