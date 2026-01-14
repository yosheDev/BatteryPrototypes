using UnityEngine;
using System.Collections.Generic;
using FunctionLibrary;
using System.Runtime.CompilerServices;

public class PlayerNeutralDetector : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private LayerMask mask;
    [HideInInspector] public bool neutralDetected = false;
    private HashSet<GameObject> neutralOverlaps = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("Default"))
        {
            neutralDetected = false;
            return;
        }

        // Trace to confirm there is not a magnet between found neutral actor and the player.
        RaycastHit2D[] hits = Physics2D.LinecastAll(batteryController.transform.position, collision.ClosestPoint(batteryController.transform.position));
        Debug.DrawLine(batteryController.transform.position, collision.ClosestPoint(batteryController.transform.position), Color.blue, 1f);
        //bool legitimate = false;
        for (int i = 0; i < hits.Length; i++)
        {
            /// If a component of the player, just ignore.
            if (hits[i].collider.gameObject.GetComponentInParent<BatteryController>() || hits[i].collider.gameObject.GetComponentInChildren<BatteryController>())
            {
                //Debug.Log("Returned on 1. Obj was " + hits[i].collider.gameObject);
                continue;
            }

            if (!FunctionLibraryF.IsInLayerMask(hits[i].collider.gameObject, mask))
            {
                Debug.Log("REturned on 2 Obj was " + hits[i].collider.gameObject);
                return;
            }
        }

        
        neutralOverlaps.Add(collision.gameObject);
        neutralDetected = true;
        Debug.Log(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
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
