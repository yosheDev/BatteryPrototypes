using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class roomObjective : MonoBehaviour
{
    AreaManager areaManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If not the player.
        if (!(collision.gameObject != null && collision.gameObject.CompareTag("Player")))
        {
            return;
        }

        gameObject.GetComponent<Collider2D>().enabled = false;

        // Begin transition animation.
        AreaManager.instance.ReachedObjective(this.gameObject);
    }
}
