using FunctionLibrary;
using Unity.Cinemachine;
using UnityEngine;

public class DollyVelocity : MonoBehaviour
{
    [SerializeField] private Vector2 zoomRange = new Vector2(10f, 15f);
    [SerializeField] private Vector2 zoomVelocityRange = new Vector2(5f, 10f);
    [SerializeField] private bool doesZoomHaveEasing = true;
    [SerializeField] private float zoomInterpEaseSpeed = 1f;
    [SerializeField] private float zoomInterpLinearSpeed = .015f;

    private BatteryController batteryController;
    private CinemachineCamera cam;          /// This is the cam that this script is attached to.
    private float zoom = 10f;
    private float additionalZoom = 0f;      /// Other scripts can modify this value for custom zoom behavior.
    private float currentZoomVelocity;

    private void Start()
    {
        batteryController = GameObject.FindAnyObjectByType<BatteryController>();
        cam = gameObject.GetComponent<CinemachineCamera>();
        zoom = CameraManager.instance.GetPositionComposer().CameraDistance;
    }
    void FixedUpdate()
    {
        if (cam.enabled)
        {
            float zoomVelocityAlpha = FunctionLibraryF.MapRangeClamped(zoomVelocityRange.x, zoomVelocityRange.y, 0f, 1f, batteryController.velocity);
            if (doesZoomHaveEasing)
            {
                zoom = Mathf.SmoothDamp(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomVelocityAlpha), ref currentZoomVelocity, zoomInterpEaseSpeed);
            }
            else
            {
                zoom = Mathf.MoveTowards(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomVelocityAlpha), zoomInterpLinearSpeed);
            }
               
            CameraManager.instance.SetCameraDistance(zoom + additionalZoom);
        }
    }

    public void SetAdditionalZoom(float extraZoom)
    {
        additionalZoom = extraZoom;
    }

    public float GetAdditionalZoom()
    {
        return additionalZoom;
    }

    public Vector2 GetZoomRange()
    {
        return zoomRange;
    }
}
