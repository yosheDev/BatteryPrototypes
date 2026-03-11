using FunctionLibrary;
using Magnet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerNeutralDetector : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private LayerMask mask;
    [HideInInspector] public bool neutralDetected = false;
    private HashSet<GameObject> neutralOverlaps = new HashSet<GameObject>();
    private Coroutine observeRoutine;

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

        neutralOverlaps.Add(collision.gameObject);
        if (observeRoutine == null)
        {
            observeRoutine = StartCoroutine(NeutralOverlapObserve());
        }
    }

    private IEnumerator NeutralOverlapObserve()
    {
        while (true)
        {
            foreach (GameObject overlapObj in neutralOverlaps)
            {
                Collider2D neutralCol = overlapObj.GetComponent<Collider2D>();

                // Get player magnet closest to neutral surface. This way so that it considers players traversal.
                Transform playerMagNearestNeutral;
                playerMagNearestNeutral = (Vector2.Distance(batteryController.positiveMag.transform.position, neutralCol.ClosestPoint(batteryController.transform.position)) > Vector2.Distance(batteryController.negativeMag.transform.position, neutralCol.ClosestPoint(batteryController.transform.position))) ? batteryController.negativeMag.transform : batteryController.positiveMag.transform;

                #region Get Nearest Distance To Magnet

                Transform playerMagNearestMagnet = null;
                MagneticSurface nearestMagSurface = null;
                List<MagnetComponentBase> positiveFields = batteryController.positiveMag.affectFields.ToList();
                List<MagnetComponentBase> negativeFields = batteryController.negativeMag.affectFields.ToList();

                float nearestMagDistance = 999999f;
                foreach (MagnetComponentBase mag in positiveFields)
                {
                    if (mag is MagneticSurface magSurface)
                    {
                        Transform curMag = (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position) > Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position)) ? batteryController.negativeMag.transform : batteryController.positiveMag.transform;
                        if (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position) < nearestMagDistance)
                        {
                            nearestMagDistance = Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position);
                            playerMagNearestMagnet = batteryController.positiveMag.transform;
                            nearestMagSurface = magSurface;
                        }

                        if (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position) < nearestMagDistance)
                        {
                            nearestMagDistance = Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position);
                            playerMagNearestMagnet = batteryController.negativeMag.transform;
                            nearestMagSurface = magSurface;
                        }
                    }
                }

                foreach (MagnetComponentBase mag in negativeFields)
                {
                    if (mag is MagneticSurface magSurface)
                    {
                        Transform curMag = (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position) > Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position)) ? batteryController.negativeMag.transform : batteryController.positiveMag.transform;
                        if (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position) < nearestMagDistance)
                        {
                            nearestMagDistance = Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.positiveMag.transform.position), batteryController.positiveMag.transform.position);
                            playerMagNearestMagnet = batteryController.positiveMag.transform;
                            nearestMagSurface = magSurface;
                        }

                        if (Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position) < nearestMagDistance)
                        {
                            nearestMagDistance = Vector2.Distance(magSurface.surfaceCol.ClosestPoint(batteryController.negativeMag.transform.position), batteryController.negativeMag.transform.position);
                            playerMagNearestMagnet = batteryController.negativeMag.transform;
                            nearestMagSurface = magSurface;
                        }
                    }
                }

                #endregion

                #region Occlusion Tracing
                float distanceToNearestNeutral = Vector2.Distance(neutralCol.ClosestPoint(playerMagNearestNeutral.position), playerMagNearestNeutral.position);
                float nearestDistanceToOccluder = 99999f;
                // Trace for magents to update nearestDistanceToOccluder.
                RaycastHit2D[] hits = Physics2D.LinecastAll(playerMagNearestNeutral.position, neutralCol.ClosestPoint(playerMagNearestNeutral.position));
                Debug.DrawLine(playerMagNearestNeutral.position, neutralCol.ClosestPoint(playerMagNearestNeutral.position), Color.red, .05f);
                for (int i = 0; i < hits.Length; i++)
                {
                    /// If should be ignored. Such as on the player or on the wrong layer.
                    if (hits[i].collider.gameObject.GetComponentInParent<BatteryController>() || hits[i].collider.gameObject.GetComponentInChildren<BatteryController>() || LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "MagneticField")
                    {
                        continue;
                    }

                    // If this is a magnet, update nearestDistanceToOccluder.
                    if (LayerMask.LayerToName(hits[i].collider.gameObject.layer) == "MagnetSurface")
                    {
                        float nearDistance = Vector2.Distance(hits[i].collider.ClosestPoint(playerMagNearestNeutral.position), playerMagNearestNeutral.position);
                        nearestDistanceToOccluder = nearDistance <= nearestDistanceToOccluder ? nearDistance : nearestDistanceToOccluder;
                        continue;
                    }
                }

                // If occluded by something, stop execution.
                if (distanceToNearestNeutral >= nearestDistanceToOccluder)
                {
                    neutralDetected = false;
                    yield return null;
                }

                // Additionally, stop execution if any player magnet is closer to a mag surface than to a neutral surface.
                if (playerMagNearestMagnet != null)
                {
                    if (Vector2.Distance(playerMagNearestMagnet.transform.position, nearestMagSurface.surfaceCol.ClosestPoint(playerMagNearestMagnet.transform.position)) < distanceToNearestNeutral)
                    {
                        neutralDetected = false;
                        yield return null;
                    }
                }

                #endregion
            }

            // If it has made it this far, then the player is on a neutral surface still.
            neutralDetected = true;
            yield return null;
        }
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
            if (observeRoutine != null)
            {
                StopCoroutine(observeRoutine);
                observeRoutine = null;
            }
        }
    }
}
