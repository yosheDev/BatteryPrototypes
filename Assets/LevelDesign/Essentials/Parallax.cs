using FunctionLibrary;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private Transform camTransform;
    private Vector3 lastCamPos;
    private float lastOrthoZoom;
    private Vector3 initialScale;
    private float initialOrthographicZoom;

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "If useNormalizedZAsFactor is true, using Z position of the object mapped within the range 0-100.";

    public bool useNormalizedZAsFactor = true;
    [Range(0f, 1f)] public float parallaxFactor = 0f;   /// In range 0-1
    
    private void Start()
    {
        camTransform = Camera.main.transform;

        initialScale = transform.localScale;
        initialOrthographicZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;
 
        lastCamPos = camTransform.position;
        lastOrthoZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;
    }
    void FixedUpdate()
    {
        #region Position
        Vector3 camDelta = camTransform.position - lastCamPos;
        Vector3 newPos;
        if (useNormalizedZAsFactor)
        {
            newPos = transform.position + (camDelta * FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z));
        }
        else
        {
            newPos = transform.position + (camDelta * parallaxFactor);
        }
        newPos.z = transform.position.z;
        transform.position = newPos;
        #endregion

        #region Scale
        float zoomDelta = CameraManager.instance._currentCamera.Lens.OrthographicSize - lastOrthoZoom; 

        // Scale object based on current camera zoom ratio
        float ratio = CameraManager.instance._currentCamera.Lens.OrthographicSize / initialOrthographicZoom;
        transform.localScale = initialScale * ratio;

        if (useNormalizedZAsFactor)
        {
            transform.localScale = Vector3.Lerp(initialScale, ((initialScale * ratio)), FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z));
        }
        else
        {
            transform.localScale = Vector3.Lerp(initialScale, ((initialScale * ratio)), parallaxFactor);
        }
        #endregion
        //transform.localScale = initialScale * ratio;
        lastCamPos = camTransform.position;
        lastOrthoZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;
    }
}
