using FunctionLibrary;
using UnityEngine;
using Unity.Cinemachine;

public class Parallax : MonoBehaviour
{
    private Transform camTransform;
    private Vector3 camInitialPos;
    private Vector3 lastCamPos;
    private float lastOrthoZoom;
    private Vector3 initialPos;
    private Vector3 initialScale;
    private float initialOrthographicZoom;

    private Vector3 parallaxPosition;

    [Header("Universal")]
    [Tooltip("Multiplies with overall effect. Can be used to enhance foreground speed.")]
    public float parallaxMultiplierTweak = 1f;      /// Is multiplied with camDelta.

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "If useNormalizedZAsFactor is true, using Z position of the object mapped within the range -10 to 100.";

    [Header("Use Z Position")]
    public bool useNormalizedZAsFactor = true;

    [Header("Custom Parallax Factor")]
    [Range(0f, 1f)] public float parallaxFactor = 0f;   /// In range 0-1

    private void OnEnable()
    {
        CinemachineCore.CameraUpdatedEvent.AddListener(ParallaxUpdate);
    }
    private void OnDisable()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(ParallaxUpdate);
    }

    private void Start()
    {
        camTransform = Camera.main.transform;
        camInitialPos = camTransform.position;

        initialPos = transform.position;
        parallaxPosition = transform.position;
        initialScale = transform.localScale;
        initialOrthographicZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;

        lastCamPos = camTransform.position;
        lastOrthoZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;

        if (transform.position.z < -10f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    private void ParallaxUpdate(CinemachineBrain brain)
    {
        //transform.position = startPos + camOffset * parallaxFactor;

        float zoomRatio = CameraManager.instance._currentCamera.Lens.OrthographicSize / initialOrthographicZoom;

        #region Position
        Vector3 camDelta = camTransform.position - lastCamPos;      /// Delta from last ParallaxUpdate().
        Vector3 camOffset = camTransform.position - camInitialPos;  /// Offset from starting position.

        Vector3 newPos;
        if (useNormalizedZAsFactor)
        {
            // Background
            if (transform.position.z >= 0f)
            {
                // First Try
                //newPos = transform.position + (camDelta * (FunctionLibraryF.MapRangeClamped(0f, 100f, zoomRatio, 1f, transform.position.z)) * (parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z)));

                // Fixed from start pos try
                //newPos = initialPos + camOffset * (FunctionLibraryF.MapRangeClamped(0f, 100f, zoomRatio, 1f, transform.position.z)) * (parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z));
                newPos = initialPos + camOffset * (parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z));

                // Third try back to deltas
                //parallaxPosition += camDelta * parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z);

                // Fourth try in viewport coords
            }
            // Foreground
            else
            {
                //newPos = transform.position + (camDelta * zoomRatio * (parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(-10f, 0f, -1f, 0f, transform.position.z)));

                newPos = initialPos + camOffset * (parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(-10f, 0f, -1f, 0f, transform.position.z));

                //parallaxPosition += camDelta * parallaxMultiplierTweak * FunctionLibraryF.MapRangeClamped(-10f, 0f, -1f, 0f, transform.position.z);
            }
        }
        else
        {
            //newPos = transform.position + (camDelta * zoomRatio * parallaxMultiplierTweak * parallaxFactor);

            newPos = initialPos + camOffset * parallaxMultiplierTweak * parallaxFactor;

            //parallaxPosition += camDelta * parallaxMultiplierTweak * parallaxFactor;

        }

        newPos.z = transform.position.z;
        transform.position = newPos;

        //parallaxPosition.z = transform.position.z;
        //transform.position = parallaxPosition;
        #endregion

        #region Scale
        //float zoomDelta = CameraManager.instance._currentCamera.Lens.OrthographicSize - lastOrthoZoom;

        // Scale object based on current camera zoom ratio
        //transform.localScale = initialScale * zoomRatio;

        //if (useNormalizedZAsFactor)
        //{
        //    transform.localScale = Vector3.Lerp(initialScale, ((initialScale * zoomRatio)), FunctionLibraryF.MapRangeClamped(0f, 100f, 0f, 1f, transform.position.z));
        //}
        //else
        //{
        //    transform.localScale = Vector3.Lerp(initialScale, ((initialScale * zoomRatio)), parallaxFactor);
        //}
        #endregion

        lastCamPos = camTransform.position;
        lastOrthoZoom = CameraManager.instance._currentCamera.Lens.OrthographicSize;
    }
}
