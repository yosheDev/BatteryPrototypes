using UnityEngine;
using FunctionLibrary;
using UnityEngine.Windows;

public class SoftwareCursor : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    private Vector2 localPos;
    private Vector2 launchControlPos; /// Used only for launch controls.
    private Vector3 worldPos = new Vector3(0f, 0f, 0f);
    public GameObject parentForPos;
	private Vector2 aimDir;
	private Vector3 launchControlMin;
    private Vector3 launchControlMax;
    private float launchControlAlpha;
    private float playerSpriteLength = 1.5f;
    
    void Update()
    {
        if (batteryController.weldState == BatteryController.WeldState.LaunchAim)
        {
            // Move along aimDir as axis. Placement falls within range that aligns with the intensity of the launch.
            launchControlMin = parentForPos.transform.position + (-(Vector3)aimDir * (parentForPos == batteryController.positiveMag.gameObject ? 1f + playerSpriteLength : 1f));
			launchControlMax = parentForPos.transform.position + (-(Vector3)aimDir * (parentForPos == batteryController.positiveMag.gameObject ? 4f + playerSpriteLength : 4f));

			launchControlPos += (batteryController.mouseDelta * .02f);
            Vector2 deltaDir = batteryController.mouseDelta.normalized;
            float deltaMag = Time.deltaTime * (batteryController.mouseDelta.magnitude / 2f);
            float deltaDot = Vector2.Dot(deltaDir, aimDir);

			if (deltaDot < -.85f)
            {
                launchControlAlpha = Mathf.Clamp(launchControlAlpha + deltaMag, 0f, 1f);
            }
            else if (deltaDot > .85f)
            {
				launchControlAlpha = Mathf.Clamp(launchControlAlpha - deltaMag, 0f, 1f);
			}

            Debug.Log(deltaMag + " | " + launchControlAlpha);

            //float dist = Vector2.Distance((Vector2)parentForPos.transform.position + launchControlPos, (Vector2)parentForPos.transform.position);
            //float distMapped = FunctionLibraryF.MapRangeClamped(0f, 6f, 0f, 1f, dist);
			// How can I use mouseDelta to move cursor along just the axis I want?

			// Get direction and magnitude of delta.

			// Apply that to the lerp alpha somehow?

			// Boil down to a float. Maybe just make delta not affect cursor and use distance clamp/check?


			transform.position = Vector3.Lerp(launchControlMin, launchControlMax, launchControlAlpha);
            return;
        }

		// Calculate and Set local offset from player character position. (Manually done to avoid it rotating with the character.)
		localPos += (batteryController.mouseDelta * .02f);
		localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
        launchControlPos = new Vector2(0f, 0f);
        launchControlAlpha = 0f;

		// Get angle between cursorObj dir and surface normal.
		aimDir = (parentForPos == batteryController.positiveMag.gameObject ? -1f : 1f) * ((Vector2)parentForPos.transform.position - ((Vector2)parentForPos.transform.position + localPos)).normalized;
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
