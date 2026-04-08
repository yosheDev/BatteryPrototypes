using UnityEngine;

public class AbilityUnlock : MonoBehaviour
{
    private Collider2D col;
    private bool obtained = false;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<BatteryController>() && !obtained)
        {
            obtained = true;
            collision.GetComponent<BatteryController>().ProgressAbility();
            Destroy(this.gameObject);
        }
    }
}
