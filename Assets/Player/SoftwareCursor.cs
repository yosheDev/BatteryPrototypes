using UnityEngine;
using UnityEngine.Windows;

public class SoftwareCursor : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    private Vector2 localPos;
    private Vector3 worldPos = new Vector3(0f, 0f, 0f);
    public GameObject parentForPos;

    void Update()
    {
        // Calculate and Set local offset from player character position. (Manually done to avoid it rotating with the character.)
        localPos += (batteryController.mouseDelta * .02f);
        localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 4f, 3.95f) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);

        // Get angle between cursorObj dir and surface normal.
        Vector2 aimDir = (parentForPos == batteryController.positiveMag.gameObject ? -1f : 1f) * ((Vector2)parentForPos.transform.position - ((Vector2)parentForPos.transform.position + localPos)).normalized;
        float angleFromNormal = (float)System.Math.Round(Mathf.Atan2(batteryController.weldSurfaceNormal.y, batteryController.weldSurfaceNormal.x) - Mathf.Atan2(aimDir.y, aimDir.x), 2);

        if (!(batteryController.weldState == BatteryController.WeldState.Welded && (angleFromNormal > batteryController.weldAngleClamp || angleFromNormal < -batteryController.weldAngleClamp)))
        {
            worldPos = (Vector2)parentForPos.transform.position + localPos;
            transform.position = worldPos;
        }
        else
        {
            localPos -= (batteryController.mouseDelta * .02f);
            localPos = batteryController.weldState == BatteryController.WeldState.Welded ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);
        }
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
