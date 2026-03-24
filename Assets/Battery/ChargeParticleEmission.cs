using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FunctionLibrary;
using UnityEngine.InputSystem.Utilities;

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
    [SerializeField] private float emissionRateVariance = .2f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private EmitDirection emitDirection = EmitDirection.X;
    [SerializeField] private Vector2 velocityRange = new Vector2(2f, 4f);
    [SerializeField] private float velocityAngleVariance = 20f;
    [SerializeField] private HingeJoint2D swayHinge;
    [SerializeField] private float swayIntensity = 1f;
    private JointMotor2D swayMotor;

    private bool randomBool = true;
    // Look into adding gravity as parameter for the particle system.

    private void Start()
    {
        _particleManager = GameObject.FindAnyObjectByType<ChargeParticleManager>();

        swayMotor = new JointMotor2D();
        UpdateMotor(1f * swayIntensity, 10000f);
        
        StartCoroutine(EmitLoop());
        StartCoroutine(SwayRandom());
    }

    private IEnumerator EmitLoop()
    {
        while (true)
        {
            Emit();
            yield return new WaitForSeconds(Mathf.Clamp(emissionRate + Random.Range(emissionRateVariance * -1f, emissionRateVariance), 0f, 30f));
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
        vel = Quaternion.Euler(0f, 0f, Random.Range(velocityAngleVariance * -1f, velocityAngleVariance)) * vel;
        emitParams.velocity = vel * Random.Range(velocityRange.x, velocityRange.y);

        _particleManager.EmitChargeParticle(emitParams, 1);
    }

    private IEnumerator SwayRandom()
    {
        while (true)
        {
            randomBool = !randomBool;
            UpdateMotor((randomBool ? 1f : -1f) * swayIntensity * Random.Range(.8f, 1f), Random.Range(9000f, 10000f));
            //yield return new WaitForSeconds(2f * Random.Range(.5f, 1.5f));
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateMotor(float motorSpeed, float maxTorque)
    {
        swayMotor.motorSpeed = motorSpeed * 100f;
        swayMotor.maxMotorTorque = maxTorque;
        swayHinge.useMotor = true;
        swayHinge.motor = swayMotor;
    }
}
