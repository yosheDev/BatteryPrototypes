
using UnityEngine;
using System.Collections.Generic;
public class MagneticTriggers : MonoBehaviour
{
    [SerializeField] private MagnetComponentBase magnetComponent;
    public bool isOnPlayer = false; /// True when this is one of the players magnets. This prevents player magnets from affecting each other.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If this trigger is not for a player, update affectfields so that player could pull/attract this magnet. Up here since would fail magtrigger check below.
        if (collision.gameObject != null && !isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Something that is not the player is detecting a player magnet.");
            //Debug.Log("Adding " + collision.gameObject + " to affectFields on " + gameObject.name);
            magnetComponent.affectFields.Add(collision.gameObject.GetComponent<MagneticTriggers>().magnetComponent);
        }

        // If this trigger is for a player magnet and the other collision is also for a player magnet.
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // If this trigger is for a player magnet and the other collision is set to not affect the player.
        if (collision.gameObject != null && isOnPlayer && collision.gameObject.GetComponent<MagneticSurface>()._canAffectPlayer == false)
        {
            return;
        }

        // If no magnetic trigger detected.
        if (collision.GetComponent<MagneticTriggers>() == null)
        {
            return;
        }

        // Add magnet component of the other object to affect fields of this gameObjects affect component.
        //Debug.Log("Adding " + collision.gameObject + " to affectFields on " + gameObject.name);
        magnetComponent.affectFields.Add(collision.GetComponent<MagneticTriggers>().magnetComponent);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Update affectfields so that player could pull/attract this magnet.
        if (collision.gameObject != null && !isOnPlayer && collision.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Something that is not the player is ending detecting a player magnet.");
            //Debug.Log("Removing " + collision.gameObject + " from affectFields on " + gameObject.name);
            magnetComponent.affectFields.Add(collision.gameObject.GetComponent<MagneticTriggers>().magnetComponent);
        }

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
