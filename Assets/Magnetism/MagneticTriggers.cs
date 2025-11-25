
using UnityEngine;
using System.Collections.Generic;
public class MagneticTriggers : MonoBehaviour
{
    [SerializeField] private MagnetComponentBase magnetComponent;
    public bool isOnPlayer = false; /// True when this is one of the players magnets. This prevents player magnets from affecting each other.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        magnetComponent.affectFields.Add(collision.gameObject.GetComponent<MagnetComponentBase>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        magnetComponent.affectFields.Remove(collision.gameObject.GetComponent<MagnetComponentBase>());
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
    }
}
