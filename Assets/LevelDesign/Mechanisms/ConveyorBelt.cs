using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private Collider2D surfaceCol;
    [SerializeField] private float rotationSpeed = 25f;

    // TO DO: Make force ignore mass. 

    private HashSet<GameObject> affectObjs = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            affectObjs.Add(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            Vector2 normal = (surfaceCol.ClosestPoint(collision.gameObject.transform.position) - (Vector2)collision.gameObject.transform.position).normalized;
            Vector2 tangent = Vector3.Cross(Vector3.forward, normal);

            Vector2 conveyorForce = tangent * rotationSpeed * Time.fixedDeltaTime;

            //collision.gameObject.GetComponent<Rigidbody2D>().AddForce(conveyorForce);
            collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity += conveyorForce;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            affectObjs.Remove(collision.gameObject);
        }
    }
}
