using UnityEngine;

public class RigidbodyBob : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobSpeed = 4f;
    public float bobStrength = 2f;

    [Header("Rotational Bobbing")]
    public float rotSpeed = 4f;
    public float rotAmount = 1f;

    private Rigidbody2D rb;
    private float originalY;
    private Quaternion startRot;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Store the baseline height the object should bob around
        originalY = transform.position.y;
        startRot = transform.localRotation;
    }

    void FixedUpdate()
    {
        float targetY = originalY + (Mathf.Sin(Time.time * bobSpeed) * bobStrength);
        float rotOffset = Mathf.Sin(Time.time * rotSpeed) * rotAmount;

        float distanceToTarget = targetY - rb.position.y;

        rb.AddForce(new Vector2(0f, distanceToTarget * bobSpeed), ForceMode2D.Force);
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, rotOffset);
    }
}