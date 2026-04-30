using Magnet;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private Collider2D hitbox;
    [SerializeField] private float hitboxDelay = .01f;
    [SerializeField] private float hitboxDuration = .1f;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
        StartCoroutine(BeginExplosion());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for environment damage interface and get back value from if this is affected by explosions or not.
        if (collision.GetComponent<IDamageable>() != null)
        {
            // If this is affected by explosions.
            if (collision.GetComponent<IDamageable>().IsAffectedByDamageType(DamageTypes.Explosives))
            {
                collision.GetComponent<IDamageable>().Damage(DamageTypes.Explosives);
            }
        }
    }

    private IEnumerator BeginExplosion()
    {
        _particleSystem.Play();
    
        yield return new WaitForSeconds(hitboxDelay);
        hitbox.enabled = true;

        yield return new WaitForSeconds(hitboxDuration);
        hitbox.enabled = false;

        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
        yield break;
    }
}
