using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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

        neutralOverlaps.Add(collision.gameObject);
        if (batteryController.positiveMag.affectFields.Count + batteryController.negativeMag.affectFields.Count <= 0)
        {
            neutralDetected = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        neutralOverlaps.Remove(collision.gameObject);
        if (neutralOverlaps.Count <= 0)
        {
            neutralDetected = false;
        }
    }

    private void Update()
    {
        Debug.Log("Neutral Detection: " + neutralDetected);
    }
}
