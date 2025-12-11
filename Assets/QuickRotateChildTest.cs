using UnityEngine;

public class QuickRotateChildTest : MonoBehaviour
{
    public GameObject parent;
    Quaternion parentLastRot;
    Vector3 parentLastPos;
    private void Start()
    {
        parentLastRot = parent.transform.rotation;
        parentLastPos = parent.transform.position;
    }
    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        Vector3 parentPivot = parent.transform.position;

        float rotAngle = Quaternion.Angle(parent.transform.rotation, parentLastRot);

        Vector3 parentPosDelta = parent.transform.position - parentLastPos;

        transform.position += parentPosDelta;
        transform.RotateAround(parentPivot, Vector3.forward, rotAngle);
       
        parentLastRot = parent.transform.rotation;
        parentLastPos = parent.transform.position;
    }
}
