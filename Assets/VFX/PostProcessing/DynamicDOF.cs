using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicDOF : MonoBehaviour
{
    private Volume volume;
    private DepthOfField depthOfField;

    void Start()
    {
        try
        {
            volume = GetComponent<Volume>();
        }
        catch
        {
            Debug.LogError("There is no volume component on " + this.gameObject.name);
        }

        if (volume.profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.active = true;
            depthOfField.focusDistance.overrideState = true;
        }
    }

    void FixedUpdate()
    {
        depthOfField.focusDistance.value = CameraManager.instance.GetCameraDistance();
    }
}
