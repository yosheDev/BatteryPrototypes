using FunctionLibrary;
using Magnet;
using System.Collections;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEngine.Splines.SplineAnimate;
using static UnityEngine.Splines.SplineComponent;

public class SplineTraversal : MonoBehaviour, IInterfaceEvent
{
    [Header("Base Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private Method moveMethod;
    [SerializeField, Range(0f, 10f), Tooltip("Speed of travel. Only used when Move Method = Speed.")] private float speed;
    [SerializeField, Tooltip("Duration of travel. Only used when Move Method = Time.")] private float duration;
    [SerializeField, Tooltip("Interpolation curve of travel.")] private AnimationCurve curve;
    [SerializeField] private float endDelay = 2f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField, Range(0f, 1f), Tooltip("Normalized distance [0;1] offset along the spline at which the GameObject should be placed when the animation begins.")]
    private float startOffset;
    [SerializeField] private bool startBackwards = false;
    [SerializeField] private bool enableRotation = false;
    
    // Constant data
    private Spline pathSpline;
    
    // Play data
    private bool isPlaying = false;
    private bool currentDirection = true;   /// true = forwards, false = backwards.
    private float currentAlpha = 0f;
    private Vector3 position;
    //private Quaternion rotation;

    private void OnValidate()
    {
        speed = Mathf.Clamp(speed, 0f, float.MaxValue);
        duration = Mathf.Clamp(duration, 0.1f, float.MaxValue);
    }
    void Start()
    {
        pathSpline = splineContainer.Spline;

        currentAlpha = startOffset;
        currentDirection = !startBackwards;

        if (playOnAwake)
        {
            Play();
        }
        else
        {
            UpdateTransform();
        }
    }

    public void Play()
    {
        isPlaying = true;
    }
    public void Pause()
    {
        isPlaying = false;
    }
    void FixedUpdate()
    {
        if (isPlaying)
        {
            var dt = Time.fixedDeltaTime;

            // Calculate spline length alpha.
            switch (moveMethod)
            {
                case Method.Speed:
                    currentAlpha += dt * (speed * (currentDirection ? 1f : -1f));
                    break;
                case Method.Time:
                    currentAlpha += (dt * FunctionLibraryF.MapRangeClamped(0f, pathSpline.GetLength(), 0f, 1f, (pathSpline.GetLength() / duration)) * (currentDirection ? 1f : -1f));
                    break;
                default:
                    break;
            }

            // Have we reached the end of spline?
            if (currentAlpha >= 1f || currentAlpha <= 0f)
            {
                EndReached();
            }

            // Exit FixedUpdate if timeline paused in last operation.
            if (!isPlaying)
            {
                return;
            }

            UpdateTransform();
        }
    }

    private void UpdateTransform()
    {
        position = transform.position;
        //rotation = Quaternion.identity;

        // Calculate and Apply transform data.
        /// Mathematics float3 vars (needed for Evaluate function.)
        float3 pos = (float3)position;
        float3 tangent = Vector3.zero;
        float3 upVector = Vector3.zero;


        pathSpline.Evaluate(currentAlpha, out pos, out tangent, out upVector);

        position = gameObject.transform.parent.transform.TransformPoint((Vector3)pos); /// Place along spline length regardless of hierarchy structure.
        position.z = transform.position.z; /// Ensure Z never changes despite spline point Z locations.
        transform.position = position;

        if (enableRotation)
        {
            float angle = 0f;
            //Debug.DrawLine(transform.position, transform.position + ((Vector3)upVector * 3f), Color.green, .2f);
           // Debug.DrawLine(transform.position, transform.position + ((Vector3)tangent * 3f), Color.red, .2f);

            angle = Mathf.Atan2(upVector.y, upVector.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0f, 0f, angle);
        }
    }

    private void EndReached()
    {
        if (pathSpline.Closed)
        {
            if (currentAlpha >= 1f)
            {
                currentAlpha = 0f;
            }
            else
            {
                currentAlpha = 1f;
            }
            currentDirection = !currentDirection; /// Gets reversed again below to = the same after this operation.
        }
        else
        {
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, 1f);
        }
            
        UpdateTransform();

        currentDirection = !currentDirection;

        if (endDelay > 0f)
        {
            StartCoroutine(EndReachedDelay());
        }
    }
    private IEnumerator EndReachedDelay()
    {
        Pause();
        yield return new WaitForSeconds(endDelay);
        Play();
        yield break;
    }

    public void InterfaceEvent(string eventName)
    {
        switch(eventName)
        {
            case "Start":
                Play();
                break;
            case "Stop":
                Pause();
                break;
        }
    }
}
