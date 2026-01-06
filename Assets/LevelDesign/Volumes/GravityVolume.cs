using System.Collections.Generic;
using UnityEngine;

public class GravityVolume : MonoBehaviour
{
    [Tooltip("Multiplies other objects gravity by this factor. Only works if gravity is not overridden.")]
    [SerializeField] private float gravityMultiplier = 1.0f;
    [Tooltip("Multiplies other objects gravity by this factor.")]
    [SerializeField] private bool overrideGravity = true;
    [Tooltip("Replaces gravity scale of other object with this value.")]

    /// Stores affected objects and their initial gravity values when first entering the volume. 
    private Dictionary<GameObject, float> affectedObjects = new Dictionary<GameObject, float>();

    [SerializeField] private float gravityOverride = 0f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // References
        GameObject otherObj = collision.gameObject;
        Rigidbody2D otherRB = otherObj.GetComponent<Rigidbody2D>();
        if (otherRB == null)
        {
            return;
        }

        // Add dictionary entry
        affectedObjects.Add(otherObj, otherRB.gravityScale);

        // Set gravity value
        otherRB.gravityScale = overrideGravity ? gravityOverride : otherRB.gravityScale * gravityMultiplier;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        // References
        GameObject otherObj = collision.gameObject;
        Rigidbody2D otherRB = otherObj.GetComponent<Rigidbody2D>();
        if (otherRB == null)
        {
            return;
        }

        // Set gravity value
        float value = 0f;
        if (affectedObjects.TryGetValue(otherObj, out value))
        {
            otherRB.gravityScale = value;
        }
        else
        {
            otherRB.gravityScale = 1f;
            Debug.LogWarning("Was not able to retrieve initial gravity value of " + otherObj + " from dictionary in GravityVolume.cs");
        }
        
        // Remove dictionary entry
        affectedObjects.Remove(otherObj);
    }
}
