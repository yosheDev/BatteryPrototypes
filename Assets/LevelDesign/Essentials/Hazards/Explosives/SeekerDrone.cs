using FunctionLibrary;
using Magnet;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class SeekerDrone : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private AIDestinationSetter destinationSetter;
    [SerializeField] private AIPath aiPath;
    [SerializeField] private MagneticSurface magSurface;
    private float _baseMaxSpeed;

    private bool affectedByPlayerMagnet = false;
    [SerializeField] private bool setPlayerAsTargetImmediately = true;

    private float originalPlayerMagInfluence;

    [Header("Stagger")]
    [SerializeField] private float _staggerDuration = 1f;
    [SerializeField] private float _staggerVelocityThreshold = .1f;
    private enum SeekerDroneState
    {
        Inactive,
        Active,
        Staggered
    }
    private SeekerDroneState droneState = SeekerDroneState.Inactive;
    private Coroutine staggerRoutine;
    private float _baseLinearDamping;

    private Vector2 lastFramePos = Vector2.zero;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        _baseLinearDamping = rb.linearDamping;

        if (destinationSetter == null)
        {
            destinationSetter = GetComponent<AIDestinationSetter>();
        }

        if (setPlayerAsTargetImmediately)
        {
            destinationSetter.target = AreaManager.instance.playerController.transform;
        }

        if (aiPath == null)
        {
            aiPath = GetComponent<AIPath>();
        }
        _baseMaxSpeed = aiPath.maxSpeed;

        if (magSurface == null)
        {
            magSurface = GetComponent<MagneticSurface>();
        }

        originalPlayerMagInfluence = magSurface._playerMagInfluence;

        droneState = SeekerDroneState.Active;
    }

    private void FixedUpdate()
    {
        //transform.position += transform.up * .1f * Time.fixedDeltaTime;

        float velocity = ((Vector2)transform.position - lastFramePos).magnitude;
        List<MagnetComponentBase> magFields = magSurface.affectFields.ToList();
        for (int i = 0; i < magFields.Count; i++)
        {
            // If the magnetic field is the players positive magnet.
            if (magFields[i] is MagneticSurface otherMagSurface)
            {
                //Debug.Log(otherMagSurface.gameObject);
                // Dot Product check - is the player mag facing towards this drone?
                //Debug.Log(otherMagSurface.gameObject + " | " + Vector2.Dot(((otherMagSurface.gameObject.transform.position - transform.position).normalized), otherMagSurface.gameObject.transform.up));
                if (Vector2.Dot(((otherMagSurface.gameObject.transform.position - transform.position).normalized), otherMagSurface.gameObject.transform.up) <= -0.5)
                {
                    // OtherMag charge is opposite of drones charge. 
                    if (otherMagSurface.IsOnPlayer() && (Mathf.Sign(otherMagSurface._magData.charge) != Mathf.Sign(magSurface._magData.charge)))
                    {
                        magSurface._playerMagInfluence = 0f;
                        aiPath.maxSpeed = _baseMaxSpeed + 1f;

                        lastFramePos = transform.position;
                        return;
                    }
                    // OtherMag charge is the same as drones charge.
                    else if (otherMagSurface.IsOnPlayer() && (Mathf.Sign(otherMagSurface._magData.charge) == Mathf.Sign(magSurface._magData.charge)))
                    {
                        aiPath.maxSpeed = 0f;

                        // If drone is just now being pushed into a staggered state.
                        if (droneState == SeekerDroneState.Active && velocity > _staggerVelocityThreshold)
                        {
                            aiPath.enabled = false;
                            rb.linearVelocity = lastFramePos - (Vector2)transform.position; /// Convert velocity to the rigidbody physics.
                            Stagger();
                        }
                    }
                }
            }

            lastFramePos = transform.position;
        }

        //if (affectedByPlayerMagnet)
        //{
        //    aiPath.maxSpeed = 0f;
        //    return;
        //}

        // Reset player mag influence (factor of how player mags affect this.)
        magSurface._playerMagInfluence = originalPlayerMagInfluence;

        if (destinationSetter.target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, destinationSetter.target.position);

            aiPath.maxSpeed = FunctionLibraryF.MapRangeClamped(10f, 15f, 1f, 3f, distanceToTarget);
        }
    }

    private void Stagger()
    {
        Debug.Log("Enter Stagger!");
        droneState = SeekerDroneState.Staggered;
        rb.linearDamping = 0f;
        
        staggerRoutine = StartCoroutine(StaggerRoutine());
    }

    private IEnumerator StaggerRoutine()
    {
        Coroutine rotRoutine = StartCoroutine(StaggerRotation());
        yield return new WaitForSeconds(_staggerDuration);
        StopCoroutine(rotRoutine);
        
        rb.linearDamping = _baseLinearDamping;
        yield return new WaitForSeconds(.5f);
        rb.linearVelocity = Vector2.zero;
        aiPath.enabled = true;
        droneState = SeekerDroneState.Active;
        yield break;
    }

    private IEnumerator StaggerRotation()
    {
        while (true)
        {
            rb.AddTorque(3f);
            yield return null;
        }
    }
}
