using UnityEngine;
using Magnet;
using System.Collections;
using System.Collections.Generic;
using FunctionLibrary;

public class a1_RisingWaterSequence : MonoBehaviour, IInterfaceEvent
{
    public GameObject waterObj;
    private SplineTraversal waterTraversal;
    [SerializeField] private float waterMinSpeed = .5f;
    [SerializeField] private float waterMaxSpeed = 1.5f;
    [Tooltip("At this distance, the water will be moving at MinSpeed.")]
    [SerializeField] private float waterSpeedBlendMinDistance = 2f;
    [Tooltip("At this distance, the water will start to blend from MaxSpeed to MinSpeed.")]
    [SerializeField] private float waterSpeedBlendMaxDistance = 4f;

    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject surfaceTopObj;

    private BatteryController playerController;
    private float lastPlayerGroundedY;

    void Start()
    {
        waterTraversal = waterObj.GetComponent<SplineTraversal>();
        playerObj = GameObject.FindAnyObjectByType<BatteryController>().gameObject;
        
        playerController = playerObj.GetComponent<BatteryController>();
        lastPlayerGroundedY = playerObj.transform.position.y;
    }

    public void InterfaceEvent(string eventName)
    {
        StartCoroutine(BeginSequence());
    }

    private IEnumerator BeginSequence()
    {
        // Camera Movements / Gurgles and stuff
        yield return new WaitForSeconds(2f);

        waterObj.GetComponent<IInterfaceEvent>().InterfaceEvent("Start");


    }

    void Update()
    {
        if (playerController.IsGrounded())
        {
            lastPlayerGroundedY = playerObj.transform.position.y;
        }

        float desiredSpeed;
        float surfaceDistanceFromPlayer;
        surfaceDistanceFromPlayer = Mathf.Abs(lastPlayerGroundedY - surfaceTopObj.transform.position.y);
        //Debug.Log(surfaceDistanceFromPlayer);

        if (surfaceDistanceFromPlayer < 10f)
        {
            desiredSpeed = FunctionLibraryF.MapRangeClamped(waterSpeedBlendMinDistance, waterSpeedBlendMaxDistance, waterMinSpeed, waterMaxSpeed, surfaceDistanceFromPlayer);
        }
        else
        {
            if (surfaceDistanceFromPlayer >= 20f)
            {
                desiredSpeed = waterMaxSpeed * 2.5f;
            }
            else
            {
                desiredSpeed = waterMaxSpeed * 1.5f;
            }
        }

        float newSpeed;
        newSpeed = Mathf.MoveTowards(waterTraversal.GetSpeed(), desiredSpeed, 0.0005f);

        waterTraversal.SetSpeed(newSpeed);
    }
}
