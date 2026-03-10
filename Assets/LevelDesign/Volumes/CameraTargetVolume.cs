using UnityEngine;

public class CameraTargetVolume : MonoBehaviour
{
    private Collider2D volumeCol;
    public Transform targetTransform;
    [SerializeField] private float blendInDuration = 1f;
    [SerializeField] private float blendOutDuration = 1f;

    private void OnValidate()
    {
        blendInDuration = Mathf.Clamp(blendInDuration, 0f, float.MaxValue);
        blendOutDuration = Mathf.Clamp(blendOutDuration, 0f, float.MaxValue);
    }
    void Start()
    {
        volumeCol = GetComponent<Collider2D>();
    }

    // TO DO: Move the actual SetAdditionalZoom to Camera Manager.
    //        Have list of overlapped volumes on Camera Manager. Use that for interpolations.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CameraManager.instance.AddFollowTarget(targetTransform, blendInDuration);
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        try
        {
            CameraManager.instance.RemoveFollowTarget(targetTransform, blendOutDuration);
        }
        catch
        {
            /// Triggers this exception when closing play mode in editor.
        }
    }
}
