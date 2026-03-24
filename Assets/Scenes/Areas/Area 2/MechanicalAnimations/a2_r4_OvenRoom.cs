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
            if (Mathf.Abs(playerObj.transform.position.x - 1.15f) < .1f)
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

        // Shut off conveyor belt.
        float currentVelocity = 0f;
        while (conveyorEffector.speed > 0f)
        {
            conveyorEffector.speed -= Mathf.SmoothDamp(conveyorEffector.speed, 0f, ref currentVelocity, 1f);
            yield return null;
        }

        // Reenable Camera Zoom
        camZoomVolume.SetActive(true);

        
    }
}
