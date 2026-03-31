using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines.Interpolators;
public class a2_r4_OvenRoom : MonoBehaviour
{
    private GameObject playerObj;
    [SerializeField] private SurfaceEffector2D conveyorEffector;
    [SerializeField] private List<GameObject> barrierObjs = new List<GameObject>();
    [SerializeField] private List<Collider2D> hallwayCamBounds = new List<Collider2D>();
    [SerializeField] private GameObject camZoomVolume;
    [SerializeField] private GameObject rotateScrap;
    [SerializeField] private Animator scrapAnimator;

    [SerializeField] private WarningIndicatorsGroupActivator topIndicators;
    [SerializeField] private WarningIndicatorsGroupActivator leftIndicators;
    [SerializeField] private WarningIndicatorsGroupActivator rightIndicators;

    [SerializeField] private RayHazardEmitterGroup topRayHazards;
    [SerializeField] private RayHazardEmitterGroup leftRayHazards;
    [SerializeField] private RayHazardEmitterGroup rightRayHazards;
    private void Start()
    {
        playerObj = GameObject.FindAnyObjectByType<BatteryController>().gameObject;
        camZoomVolume.SetActive(false);
        foreach (GameObject obj in barrierObjs)
        {
            obj.SetActive(false);
        }

        StartCoroutine(BeginSequence());
    }

    public IEnumerator BeginSequence()
    {
        // Wait until player is within trigger bounds.
        while (true)
        {
            if (playerObj.transform.position.x > 1.15f  && playerObj.transform.position.y > 13f)
            {
                break;
            }

            yield return null;
        }

        // Activate barriers.
        foreach (GameObject obj in barrierObjs)
        {
            obj.SetActive(true);
        }

        // Disable camera bounds in hallways.
        foreach (Collider2D col in hallwayCamBounds)
        {
            col.gameObject.SetActive(false);
            CameraManager.instance.UpdateConfinedBounds();
        }

        // Reenable Camera Zoom
        camZoomVolume.SetActive(true);

        topIndicators.FlashIndicators(6f, .25f, .125f);
        yield return new WaitForSeconds(6f);
        //Debug.Log("Macrowave from the TOP");
        topRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        topRayHazards.EmitEnd();

        yield return new WaitForSeconds(1f);

        rightIndicators.FlashIndicators((playerObj.transform.position.x >= 13.5) ? 9f : 5.5f, .25f, .125f); /// Gives players more time if they have fallen way to the left.
        yield return new WaitForSeconds((playerObj.transform.position.x >= 13.5) ? 9f : 5.5f);
        //Debug.Log("Macrowave from the RIGHT");
        rightRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        rightRayHazards.EmitEnd();

        yield return new WaitForSeconds(1f);

        leftIndicators.FlashIndicators((playerObj.transform.position.x <= -11.5) ? 8f : 5f, .25f, .125f); /// Gives players more time if they fallen way to the left.
        yield return new WaitForSeconds((playerObj.transform.position.x <= -11.5) ? 8f : 5f);
        //Debug.Log("Macrowave from the LEFT");
        leftRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        leftRayHazards.EmitEnd();

        yield return new WaitForSeconds(1f);

        Debug.Log("Should have set the parameter to be fucking 1.");
        scrapAnimator.SetInteger("sequenceState", 1);

        yield return new WaitForSeconds(3f);

        leftIndicators.FlashIndicators(5f, .25f, .125f); 
        rightIndicators.FlashIndicators(5f, .25f, .125f);
        yield return new WaitForSeconds(5f);
        //Debug.Log("Macrowave from the LEFT and RIGHT");
        leftRayHazards.EmitBegin();
        rightRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        leftRayHazards.EmitEnd();
        rightRayHazards.EmitEnd();

        yield return new WaitForSeconds(.5f);

        topIndicators.FlashIndicators(7f, .25f, .125f);
        rightIndicators.FlashIndicators(7f, .25f, .125f);
        yield return new WaitForSeconds(7f);
        //Debug.Log("Macrowave from the RIGHT and TOP");
        topRayHazards.EmitBegin();
        rightRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        topRayHazards.EmitEnd();
        rightRayHazards.EmitEnd();

        yield return new WaitForSeconds(.5f);

        topIndicators.FlashIndicators(10f, .25f, .125f);
        leftIndicators.FlashIndicators(10f, .25f, .125f);
        yield return new WaitForSeconds(11f);
        //Debug.Log("Macrowave from the LEFT and TOP");
        topRayHazards.EmitBegin();
        leftRayHazards.EmitBegin();
        yield return new WaitForSeconds(5f);
        topRayHazards.EmitEnd();
        leftRayHazards.EmitEnd();

        yield return new WaitForSeconds(.5f);
        topIndicators.FlashIndicators(3f, .25f, .125f);
        leftIndicators.FlashIndicators(3f, .25f, .125f);
        leftIndicators.FlashIndicators(3f, .25f, .125f);
        yield return new WaitForSeconds(3f);
        // Macrowave breaks and shuts down.

        // Disable barriers.
        foreach (GameObject obj in barrierObjs)
        {
            obj.SetActive(false);
        }

        // Enable camera bounds in hallways.
        foreach (Collider2D col in hallwayCamBounds)
        {
            col.gameObject.SetActive(true);
            CameraManager.instance.UpdateConfinedBounds();
        }
    }







    // Turn on conveyor belt.
    //float currentVelocity = 0f;
    //    while (conveyorEffector.speed < 8f)
    //    {
    //        conveyorEffector.speed -= Mathf.SmoothDamp(conveyorEffector.speed, 8f, ref currentVelocity, 1f);
    //        yield return null;
    //    }
}
