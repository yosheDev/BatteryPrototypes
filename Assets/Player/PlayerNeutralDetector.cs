using FunctionLibrary;
using Magnet;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerNeutralDetector : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private LayerMask mask;
    [HideInInspector] public bool neutralDetected = false;
    private HashSet<GameObject> neutralOverlaps = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            // Note: Interaction currently not compatable with player overlapping several interactables. I doubt we will every have interactables that close to eachother, so shouldn't be an issue.
            try
            {
                IInteractable interactObj = collision.gameObject.GetComponent<IInteractable>();
                batteryController.interactObj = interactObj;
                interactObj.TrySetInteractDisplay(true);
            }
            catch { }

            return;
        }


        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("ElectricNode"))
        {
            return;
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("Default"))
        {
            neutralDetected = false;
            return;
        }

        #region Occlusion Tracing
        // Trace to confirm there is not a magnet between found neutral actor and the player.
        RaycastHit2D[] hits = Physics2D.LinecastAll(batteryController.transform.position, collision.ClosestPoint(batteryController.transform.position));
        float nearestDistanceToOccluder = 99999f;
        float distanceToNearestNeutral = Vector2.Distance(collision.ClosestPoint(batteryController.gameObject.transform.position), batteryController.gameObject.transform.position);
        for (int i = 0; i < hits.Length; i++)
        {
            /// If a component of the player, just ignore.
            if (hits[i].collider.gameObject.GetComponentInParent<BatteryController>() || hits[i].collider.gameObject.GetComponentInChildren<BatteryController>())
            {
                continue;
            }

            // Is this hit an occluder? If so, track distance.
            if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "MagnetSurface")
            {
                //Debug.Log("Occluder: " + hits[i].collider.gameObject);
                float nearDistance = Vector2.Distance(hits[i].collider.ClosestPoint(batteryController.gameObject.transform.position), batteryController.gameObject.transform.position);
                nearestDistanceToOccluder = nearDistance <= nearestDistanceToOccluder ? nearDistance : nearestDistanceToOccluder;
                continue;
            }

            // Is a closer neutral surface detected? If so, track distance. (This might be useless?)
            //if (FunctionLibraryF.IsInLayerMask(hits[i].collider.gameObject, mask))
            //{
            //    float nearDistance = Vector2.Distance(hits[i].collider.ClosestPoint(batteryController.gameObject.transform.position), batteryController.gameObject.transform.position);
            //    distanceToNearestNeutral = nearDistance <= distanceToNearestNeutral ? nearDistance : distanceToNearestNeutral;
            //    continue;
            //}
            //else
            //{
            //    Debug.Log(hits[i].collider.gameObject + " was not in layerMask");
            //}

        }

        // If occluded by something, stop execution.
        //Debug.Log("Neutral near distance: " + distanceToNearestNeutral + " | Occluder distance: " + nearestDistanceToOccluder);
        if (distanceToNearestNeutral >= nearestDistanceToOccluder)
        {
            return;
        }
        #endregion

        neutralOverlaps.Add(collision.gameObject);
        neutralDetected = true;
        //Debug.Log("Neutral Added: " + collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            // Note: Interaction currently not compatable with player overlapping several interactables. I doubt we will every have interactables that close to eachother, so shouldn't be an issue.
            try
            {
                IInteractable interactObj = collision.gameObject.GetComponent<IInteractable>();
                interactObj.TrySetInteractDisplay(false);
            }
            catch { }

            return;
        }

        neutralOverlaps.Remove(collision.gameObject);
        if (neutralOverlaps.Count <= 0)
        {
            neutralDetected = false;
        }
    }

    //private void Update()
    //{
    //    Debug.Log("Neutral Detection: " + neutralDetected);
    //}
}
