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

        topIndicators.FlashIndicators(5f, .25f, .125f);
        yield return new WaitForSeconds(5f);
        Debug.Log("Macrowave from the TOP");

        yield return new WaitForSeconds(5f);

        rightIndicators.FlashIndicators(5f, .25f, .125f);
        yield return new WaitForSeconds(5f);
        Debug.Log("Macrowave from the RIGHT");

        yield return new WaitForSeconds(5f);

        leftIndicators.FlashIndicators(5f, .25f, .125f);
        yield return new WaitForSeconds(4f);
        Debug.Log("Macrowave from the LEFT");
    }







    // Turn on conveyor belt.
    //float currentVelocity = 0f;
    //    while (conveyorEffector.speed < 8f)
    //    {
    //        conveyorEffector.speed -= Mathf.SmoothDamp(conveyorEffector.speed, 8f, ref currentVelocity, 1f);
    //        yield return null;
    //    }
}
