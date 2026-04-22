using UnityEngine;

public class GrappleRendererVFX : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform[] points = new Transform[2];
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        if (lineRenderer.enabled == true)
        {
            try
            {
                Vector3[] pos = new Vector3[] { points[0].position, points[1].position };
                lineRenderer.SetPositions(pos);
            }
            catch
            {
                lineRenderer.enabled = false;
            }
        }
    }
    public void SetPoints(Transform[] newPoints)
    {
        points = newPoints;
    }

    public void SetState(bool state)
    {
        if (state)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}
