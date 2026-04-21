using UnityEngine;
using UnityEngine.U2D;

public class SpriteShapeTransformMapper : MonoBehaviour
{
    public SpriteShapeController spriteShapeController;
    public Transform[] segments;

    void LateUpdate()
    {
        var spline = spriteShapeController.spline;

        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 localPos = transform.InverseTransformPoint(segments[i].position);
            spline.SetPosition(i, localPos);
        }
    }
}
