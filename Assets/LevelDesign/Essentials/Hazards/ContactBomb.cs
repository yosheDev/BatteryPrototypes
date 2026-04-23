using UnityEngine;

public class ContactBomb : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BatteryController>() != null || collision.gameObject.GetComponent<Corrosion>() != null)
        {
            Debug.Log("BOOM!");
            Destroy(this.gameObject);
        }
    }
}
