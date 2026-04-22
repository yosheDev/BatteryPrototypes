using UnityEngine;
using UnityEngine.U2D;

public class SpriteShapeTransformMapper : MonoBehaviour
{
    public SpriteShapeController spriteShapeController;
    public Transform[] segments;

    [Tooltip("If true, the end of the spline will be treated as if it were not connected to any anchor. As if hanging like a rope.")]
    [SerializeField] private bool isEndLoose = false;
    private Vector2 endOffset;
    private void Start()
    {
        SetEndLoose(isEndLoose);
    }
    void Update()
    {
        var spline = spriteShapeController.spline;

        for (int i = 0; i < segments.Length; i++)
        {
            Vector3 localPos = transform.InverseTransformPoint(segments[i].position);
            spline.SetPosition(i, localPos);
        }

        if (isEndLoose)
        {
            spline.SetPosition(spriteShapeController.spline.GetPointCount() - 1, spriteShapeController.spline.GetPosition(spriteShapeController.spline.GetPointCount() - 2) + segments[spriteShapeController.spline.GetPointCount() - 2].up * endOffset.magnitude);
        }
    }

    public void SetEndLoose(bool state = false)
    {
        if (state)
        {
            endOffset = (Vector2)spriteShapeController.spline.GetPosition(spriteShapeController.spline.GetPointCount() - 1) - (Vector2)spriteShapeController.spline.GetPosition(spriteShapeController.spline.GetPointCount() - 2);
        }
    }
}
