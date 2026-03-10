using UnityEngine;

public class ElectricLineRendererUpdate : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform startTrans;
    private Transform endTrans;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void FixedUpdate()
    {
        try
        {
            lineRenderer.SetPosition(0, startTrans.position);
            lineRenderer.SetPosition(1, endTrans.position);
        }
        catch
        {
            // StartTrans / EndTrans are null somehow? Not sure how though, since they are never destroyed.
        }
    }

    public void SetStartPointParent(Transform newTrans)
    {
        startTrans = newTrans;
    }
    public void SetEndPointParent(Transform newTrans)
    {
        endTrans = newTrans;
    }
}
