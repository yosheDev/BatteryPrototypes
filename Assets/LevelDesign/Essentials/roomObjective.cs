using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class roomObjective : MonoBehaviour
{
    AreaManager areaManager;
    [SerializeField] private PointEffector2D pointEffector;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If not the player.
        if (!(collision.gameObject != null && collision.gameObject.CompareTag("Player")))
        {
            return;
        }
        pointEffector.enabled = false;
        collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        collision.gameObject.GetComponent<Rigidbody2D>().Sleep();
        gameObject.GetComponent<Collider2D>().enabled = false;

        // Begin transition animation.
        AreaManager.instance.ReachedObjective(this.gameObject);
    }
}
