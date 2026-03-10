using UnityEngine;

public class CameraZoomVolume : MonoBehaviour
{
    private Collider2D volumeCol;
    public float additionalZoom = 0f;
    public float transitionDuration = 4f;
    void Start()
    {
        volumeCol = GetComponent<Collider2D>();
    }

    // TO DO: Move the actual SetAdditionalZoom to Camera Manager.
    //        Have list of overlapped volumes on Camera Manager. Use that for interpolations.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CameraManager.instance.AddZoomVolume(this);
        CameraManager.instance.BlendCameraZoomVolume(true, additionalZoom, transitionDuration);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        try
        {
            CameraManager.instance.RemoveZoomVolume(this);
            CameraManager.instance.BlendCameraZoomVolume(false, 0f, transitionDuration);
        }
        catch
        {
            /// Triggers this exception when closing play mode in editor.
        }
    }
}
