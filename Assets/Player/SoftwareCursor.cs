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
        localPos = Vector2.ClampMagnitude(localPos, 2f);
        worldPos = (Vector2)parentForPos.transform.position + localPos;
        transform.position = worldPos;
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
