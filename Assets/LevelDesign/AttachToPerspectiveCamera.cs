using UnityEngine;

public class AttachToPerspectiveCamera : MonoBehaviour
{
    void Start()
    {
        this.gameObject.transform.parent = Camera.main.transform;
    }
}
