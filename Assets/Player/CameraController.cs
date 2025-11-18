using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject followTarget;
    public Vector3 velocity = new Vector3(0, 0, 0);
    [SerializeField] private float smoothTime = 15f;
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 followPos = Vector3.SmoothDamp(transform.position, followTarget.transform.position, ref velocity, smoothTime);//Vector3.Slerp(transform.position, followTarget.transform.position, 8f);
        transform.position = new Vector3(followPos.x, followPos.y, -10f); 
    }
}
