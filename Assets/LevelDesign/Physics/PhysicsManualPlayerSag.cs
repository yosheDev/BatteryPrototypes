using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class PhysicsManualPlayerSag : MonoBehaviour
{
    [SerializeField] private Collider2D spriteShapeCol;
    [SerializeField] private float playerForceFactor = 10f;
    [SerializeField] private List<Transform> bones;
    private int boneIndex = 0;

    Rigidbody2D rb;
    private Vector2 restPosition;

    [SerializeField] private float springStrength = 8f;
    [SerializeField] private float damping = 3f;

    private void Start()
    {
        rb = transform.parent.gameObject.GetComponent<Rigidbody2D>();
        restPosition = rb.position;

        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i].gameObject == rb.gameObject)
            {
                boneIndex = i;
                break;
            }
        }
    }

    private void FixedUpdate()
    {
        // Spring back to resting pos
        Vector2 offset = restPosition - rb.position;
        rb.AddForce(offset * springStrength + (-rb.linearVelocity * damping));
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<BatteryController>() != null)
        {
            ApplyPlayerWeight(collision);
        }
    }
    private void ApplyPlayerWeight(Collider2D playerCol)
    {
        Vector2 totalForce = Vector2.zero;

        Vector3 playerClosestPos = playerCol.ClosestPoint(transform.position);
        float dist = Vector2.Distance((Vector2)playerClosestPos, (Vector2)spriteShapeCol.ClosestPoint(playerClosestPos));
        float t = Mathf.Clamp01(dist / .5f);

        t = 1f - (t * t);

        totalForce += (spriteShapeCol.ClosestPoint(playerClosestPos) - (Vector2)playerClosestPos).normalized * (playerForceFactor * t);

        //Debug.DrawLine(transform.position, transform.position + (Vector3)(totalForce.normalized * 3f), Color.red, .2f);

        // Damping
        totalForce += -rb.linearVelocity * damping;


        rb.AddForce(totalForce);

        // Add Force to Neighbors
        if (boneIndex - 1 >= 0)
        {
            bones[boneIndex - 1].GetComponent<Rigidbody2D>().AddForce(totalForce * 0.5f);
        }

        if (boneIndex + 1 <= bones.Count - 1)
        {
            bones[boneIndex + 1].GetComponent<Rigidbody2D>().AddForce(totalForce * 0.5f);
        }
    }
}
