using UnityEngine;

public class CorrosiveDrip : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * -4f);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject);
    }
}
