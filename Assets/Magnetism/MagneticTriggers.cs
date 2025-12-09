
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

        if (collision.GetComponent<MagneticTriggers>() == null)
        {
            return;
        }
        // Add magnet component of the other object to affect fields of this gameObjects affect component.
        magnetComponent.affectFields.Add(collision.GetComponent<MagneticTriggers>().magnetComponent);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.GetComponent<MagneticTriggers>() == null)
        {
            return;
        }
        // Remove magnet component of the other object from affect fields of this gameObjects affect component.
        magnetComponent.affectFields.Remove(collision.GetComponent<MagneticTriggers>().magnetComponent);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }
    }
}
