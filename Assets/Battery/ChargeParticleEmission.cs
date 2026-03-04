using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargeParticleEmission : MonoBehaviour
{
    private enum EmitDirection
    {
        X,
        XMinus,
        Y,
        YMinus
    }

    private ChargeParticleManager _particleManager;

    [Header("Parameters")]
    [SerializeField] private float emissionRate = .5f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private EmitDirection emitDirection = EmitDirection.X;
    [SerializeField] private float velocityAngleRadius = 10f;
    [SerializeField] private Vector2 velocityRange = new Vector2(1f, 1.5f);
    [SerializeField] private float angleVariance;
    // Look into adding gravity as parameter for the particle system.

    private void Start()
    {
        _particleManager = GameObject.FindFirstObjectByType<ChargeParticleManager>();
        StartCoroutine(EmitLoop());
    }

    private IEnumerator EmitLoop()
    {
        while (true)
        {
            Emit();
            yield return new WaitForSeconds(emissionRate);
        }
    }

    void Emit()
    {
        // Set start parameters for particle system module.
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = Color.green;
        emitParams.startSize = Random.Range(_particleManager.chargeParGlobalSizeRange.x, _particleManager.chargeParGlobalSizeRange.y);
        emitParams.startLifetime = 10f;
        emitParams.position = transform.position;

        Vector3 vel;
        switch (emitDirection)
        {
            case EmitDirection.X:
                vel = transform.right;
                break;
            case EmitDirection.XMinus:
                vel = transform.right * -1f;
                break;
            case EmitDirection.Y:
                vel = transform.up;
                break;
            case EmitDirection.YMinus:
                vel = transform.up * -1f;
                break;
            default:
                vel = transform.right;
                break;
        }
        emitParams.velocity = vel * Random.Range(velocityRange.x, velocityRange.y);

        _particleManager.EmitChargeParticle(emitParams, 1);
    }
}
