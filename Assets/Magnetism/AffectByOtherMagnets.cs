using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AffectByOtherMagnets : MonoBehaviour
{
    // When this is script is placed on magnet with a rigidbody, it will allow the magnet to be pushed by other magnets.

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MagneticSurface magnetScript;
    [SerializeField] private MagneticTriggers field;
    [SerializeField] private Collider2D surfaceCol;

    private void FixedUpdate()
    {
        Vector2 combinedForces = Vector2.zero;

        List<MagnetComponentBase> affectFields = magnetScript.affectFields.ToList();
        foreach (MagnetComponentBase mag in affectFields)
        {
            Vector2 curForce = mag.GetAppliedForce(magnetScript._magData, magnetScript.transform.position, magnetScript._fieldAttractDistance, rb.linearVelocity);

            // Prevent NaN
            if (float.IsNaN(curForce.x))
            {
                curForce = Vector2.zero;
            }

            combinedForces += curForce;
        }

        rb.AddForce(combinedForces);
    }
}
