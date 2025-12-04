using UnityEngine;
using FunctionLibrary;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject followTarget;
    [SerializeField] private BatteryController batteryController;
    [SerializeField] private Camera orthoCam;
    private Vector3 velocity = new Vector3(0, 0, 0);
    [Header("Lag")]
    [SerializeField] private float smoothTime = 15f;
    [Header("Zoom")]
    [SerializeField] private Vector2 zoomRange = new Vector2(8f, 14f);
    [SerializeField] private Vector2 zoomVelocityRange = new Vector2(2f, 10f);
    [SerializeField] private float zoomInterpSpeed = .01f;
    private float zoom;

    private void Start()
    {
        if (orthoCam == null)
        {
            orthoCam = gameObject.GetComponent<Camera>();
        }
        zoom = orthoCam.orthographicSize;
    }
    void FixedUpdate()
    {
        Vector3 followPos = Vector3.SmoothDamp(transform.position, followTarget.transform.position, ref velocity, smoothTime);//Vector3.Slerp(transform.position, followTarget.transform.position, 8f);
        float zoomVelocityAlpha = FunctionLibraryF.MapRangeClamped(zoomVelocityRange.x, zoomVelocityRange.y, 0f, 1f, batteryController.velocity);
        zoom = Mathf.MoveTowards(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomVelocityAlpha), zoomInterpSpeed);
        orthoCam.orthographicSize = zoom;
        //Debug.Log(zoom);
        transform.position = new Vector3(followPos.x, followPos.y, -10f); 
    }
}
