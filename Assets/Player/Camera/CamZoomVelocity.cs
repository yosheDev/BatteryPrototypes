using FunctionLibrary;
using Unity.Cinemachine;
using UnityEngine;

public class DollyVelocity : MonoBehaviour
{
    [SerializeField] private Vector2 zoomRange = new Vector2(8f, 14f);
    [SerializeField] private Vector2 zoomVelocityRange = new Vector2(2f, 10f);
    [SerializeField] private float zoomInterpSpeed = .05f;

    private BatteryController batteryController;
    private CinemachineCamera cam; /// This is the cam that this script is attached to.
    private float zoom = 10f;

    private void Start()
    {
        batteryController = GameObject.FindFirstObjectByType<BatteryController>();
        cam = gameObject.GetComponent<CinemachineCamera>();
        zoom = CameraManager.instance.GetPositionComposer().CameraDistance;
    }
    void FixedUpdate()
    {
        if (cam.enabled)
        {
            float zoomVelocityAlpha = FunctionLibraryF.MapRangeClamped(zoomVelocityRange.x, zoomVelocityRange.y, 0f, 1f, batteryController.velocity);
            zoom = Mathf.MoveTowards(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomVelocityAlpha), zoomInterpSpeed);
            //Debug.Log(zoom);
            CameraManager.instance.SetCameraDistance(zoom);
        }
    }
}
