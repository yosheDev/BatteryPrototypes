using UnityEngine;

public class AttachToPerspectiveCamera : MonoBehaviour
{
    void Start()
    {
        this.gameObject.transform.parent = CameraManager.instance.perspectiveCam.transform;
    }
}
