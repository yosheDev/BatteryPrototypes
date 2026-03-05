using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ElectricNodeDetector : MonoBehaviour
{
    [SerializeField] private ElectricNode nodeScript;

    private void Awake()
    {
        List<Collider2D> results = new List<Collider2D>();
        GetComponent<Collider2D>().Overlap(results);    
        foreach (Collider2D col in results)
        {
            if (col.gameObject.CompareTag("ElectricNode"))
            {
                nodeScript.withinRangeNodes.Add(col.gameObject.GetComponent<ElectricNode>());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ElectricNode"))
        {
            nodeScript.withinRangeNodes.Add(collision.gameObject.GetComponent<ElectricNode>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ElectricNode"))
        {
            nodeScript.RemoveRangeNode(collision.gameObject.GetComponent<ElectricNode>());
        }
    }
}
