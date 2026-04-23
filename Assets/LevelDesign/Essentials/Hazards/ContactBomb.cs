using UnityEngine;

public class ContactBomb : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("BOOM!");
        Destroy(this.gameObject);
    }
}
