using UnityEngine;
using UnityEngine.Windows;

public class SoftwareCursor : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    private Vector2 localPos;
    private Vector3 worldPos = new Vector3(0f, 0f, 0f);
    public GameObject parentForPos;
    
    // Cling Controls
    [HideInInspector]public bool clampMin = false;

    void Update()
    {
        // TO DO: Constrain in cone when player is clinging.

        


        // Calculate and Set local offset from player character position. (Manually done to avoid it rotating with the character.)
        localPos += (batteryController.mouseDelta * .02f);
        localPos = clampMin ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);

        // Get angle to center.
        Vector2 aimDir = (parentForPos == batteryController.positiveMag.gameObject ? -1f : 1f) * (((Vector2)parentForPos.transform.position + localPos) - (Vector2)parentForPos.transform.position).normalized;
        float angleFromNormal = (float)System.Math.Round(Mathf.Atan2(aimDir.y, aimDir.x) - Mathf.Atan2(-batteryController.clingSurfaceNormal.y, -batteryController.clingSurfaceNormal.x));
        Debug.Log(angleFromNormal + " | " + batteryController.clingAngleClamp);

        //if (clampMin && (angleFromNormal > batteryController.clingAngleClamp || angleFromNormal < -batteryController.clingAngleClamp))
        //{
           
        //}
        //else
        //{
            worldPos = (Vector2)parentForPos.transform.position + localPos;
            Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)aimDir * 3f), Color.green, .2f);
            Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + (-(Vector3)batteryController.clingSurfaceNormal * 3f), Color.purple, .2f);
            transform.position = worldPos;
        //}
    }

    // Custom ClampMagnitude that includes min and max both.
    public Vector2 ClampMagnitudeRange(Vector2 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if (sm > (double)max * (double)max) return v.normalized * max;
        else if (sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
}
