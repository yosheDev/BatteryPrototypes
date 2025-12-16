using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParentSurfaceDetector : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private LayerMask mask;
    private HashSet<GameObject> surfaceParents = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("Default"))
        {
            return;
        }

        surfaceParents.Add(collision.gameObject);
        UpdateHierarchy();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        surfaceParents.Remove(collision.gameObject);
        UpdateHierarchy();
    }

    private void UpdateHierarchy()
    {
        if (surfaceParents.Count <= 0)
        {
            batteryController.ClearParentSource();
        }
        else
        {
            List<GameObject> parents = surfaceParents.ToList();
            // For now, just supports one parent at a time. Unsure if will ever need multiple but that will be a challenge for a different time.
            batteryController.AddParentSource(parents[0]);
        }
    }
}
