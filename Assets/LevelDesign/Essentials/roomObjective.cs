using UnityEngine;

public class roomObjective : MonoBehaviour
{
    AreaManager areaManager;
    void Start()
    {
        areaManager = GameObject.FindFirstObjectByType<AreaManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If not the player.
        if (!(collision.gameObject != null && collision.gameObject.CompareTag("Player")))
        {
            return;
        }

        gameObject.GetComponent<Collider2D>().enabled = false;
        areaManager.LoadNextRoom();
    }
}
