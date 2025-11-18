
using UnityEngine;
using System.Collections.Generic;
public class MagneticTriggers : MonoBehaviour
{
    [SerializeField] private MagnetComponentBase magnetComponent;
    [SerializeField] private bool isOnPlayer = false; /// True when this is one of the players magnets. This prevents player magnets from affecting each other.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        magnetComponent.affectFields.Add(collision.gameObject.GetComponent<MagnetComponentBase>());
        //Debug.Log("Add Field ");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        //Debug.Log("Remove Field ");
        magnetComponent.affectFields.Remove(collision.gameObject.GetComponent<MagnetComponentBase>());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
    }
}
