using UnityEngine;
using FunctionLibrary;
using UnityEngine.Windows;
using UnityEditor.Rendering;
using System.Collections;

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
    private bool justWelded = false;
    private Quaternion weldInitialQuat;
    private Quaternion targetQuat;
    private float correctWeldAlpha = 0f;

    void Update()
    {
        #region Launch Aim Controls
        if (batteryController.weldState == BatteryController.WeldState.LaunchAim)
        {
            // Move along aimDir as axis. Placement falls within range that aligns with the intensity of the launch.
            launchControlMin = parentForPos.transform.position + (-(Vector3)aimDir * (parentForPos == batteryController.positiveMag.gameObject ? 1f + playerSpriteLength : 1f));
			launchControlMax = parentForPos.transform.position + (-(Vector3)aimDir * (parentForPos == batteryController.positiveMag.gameObject ? 4f + playerSpriteLength : 4f));
            
            // Will be replaced with graphics later.
            Debug.DrawLine(launchControlMin, launchControlMax, Color.white, Time.deltaTime);

			launchControlPos += (batteryController.mouseDelta * .02f);
            Vector2 deltaDir = batteryController.mouseDelta.normalized;
            float deltaMag = Time.deltaTime * (batteryController.mouseDelta.magnitude / 2f);
            float deltaDot = Vector2.Dot(deltaDir, aimDir);

            // TO DO: Make this get harder to compress towards the Max end to feel "springy".
            //          Make deltaMags amount change based on it. Make a formula it is put through that transforms it based on current launchControlAlpha.
			if (deltaDot < -.85f)
            {
                launchControlAlpha = Mathf.Clamp(launchControlAlpha + DeltaMagSpringFormula(deltaMag, launchControlAlpha), 0f, 1f);
            }
            else if (deltaDot > .85f)
            {
				launchControlAlpha = Mathf.Clamp(launchControlAlpha - DeltaMagSpringFormula(deltaMag, launchControlAlpha, true), 0f, 1f);
			}

            //Debug.Log(deltaMag + " | " + launchControlAlpha);

            //float dist = Vector2.Distance((Vector2)parentForPos.transform.position + launchControlPos, (Vector2)parentForPos.transform.position);
            //float distMapped = FunctionLibraryF.MapRangeClamped(0f, 6f, 0f, 1f, dist);

			transform.position = Vector3.Lerp(launchControlMin, launchControlMax, launchControlAlpha);
            return;
        }
        #endregion

        #region Local Software Cursor (Non-Launching)
        // Calculate and Set local offset from player character position. (Manually done to avoid it rotating with the character.)
        if (!justWelded)
        {
            localPos += (batteryController.mouseDelta * .02f);
            localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
        }
        launchControlPos = new Vector2(0f, 0f);
        launchControlAlpha = 0f;

        // Get angle between cursorObj dir and surface normal.
        aimDir = (parentForPos == batteryController.positiveMag.gameObject ? -1f : 1f) * ((Vector2)parentForPos.transform.position - ((Vector2)parentForPos.transform.position + localPos)).normalized;
        float angleFromNormal = (float)System.Math.Round(Mathf.Atan2(batteryController.weldSurfaceNormal.y, batteryController.weldSurfaceNormal.x) - Mathf.Atan2(aimDir.y, aimDir.x), 2);

        if (((batteryController.weldState == BatteryController.WeldState.Welded && (angleFromNormal > batteryController.weldAngleClamp || angleFromNormal < -batteryController.weldAngleClamp))))
        {
            // Clamp localPos back within range.
            if (justWelded)
            {

                #region New Interp Method
                //Quaternion curRot = Quaternion.Slerp(weldInitialQuat, targetQuat, correctWeldAlpha);

                //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((curRot * -(Vector3)batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.red, .5f);
                //transform.position = parentForPos.transform.position + ((curRot * -(Vector3)batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f));

                // Things break when localPos stuff is uncommented. Related to aimDir im sure.
                //localPos = -parentForPos.transform.InverseTransformPoint(transform.position);
                //localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
                #endregion

                #region Old TP Method
                //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((-(Vector3)batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.red, .5f);
                
                Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((targetQuat * (-(Vector3)batteryController.weldSurfaceNormal)) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.red, .5f);
                transform.position = parentForPos.transform.position + ((targetQuat * (-(Vector3)batteryController.weldSurfaceNormal)) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f));
                batteryController.GetRigidBody().MoveRotation(Quaternion.LookRotation(Vector3.forward, targetQuat * (-(Vector3)batteryController.weldSurfaceNormal)));
                // Local stuff uncommented breaks shit.
                //localPos = -parentForPos.transform.InverseTransformPoint(transform.position);
                //localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);

                // Handle cursor position data.
                //transform.position = parentForPos.transform.position + ((-(Vector3)batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f));    
                //localPos = -parentForPos.transform.InverseTransformPoint(transform.position);
                //localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
                #endregion
            }
            else
            {
                localPos -= (batteryController.mouseDelta * .02f);
                localPos = batteryController.weldState == BatteryController.WeldState.Welded ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);
            }  
        }
        else
        {
            if (justWelded)
            {
                justWelded = false;
                //StopCoroutine
            }
            else
            {
                //Debug.Log("Normal Controls");
                worldPos = (Vector2)parentForPos.transform.position + localPos;
                transform.position = worldPos;
            }  
        }
        #endregion
    }

    // Custom ClampMagnitude that includes min and max both.
    public Vector2 ClampMagnitudeRange(Vector2 v, float max, float min)
    {
        double sm = v.sqrMagnitude;
        if (sm > (double)max * (double)max) return v.normalized * max;
        else if (sm < (double)min * (double)min) return v.normalized * min;
        return v;
    }
    private float DeltaMagSpringFormula(float deltaMag, float springAlpha, bool isReleasing = false)
    {
        // Releasing : Winding Up
        float mult = isReleasing ? FunctionLibraryF.MapRangeClamped(1f, .5f, 2f, 1f, springAlpha) : FunctionLibraryF.MapRangeClamped(.5f, 1f, 1f, .1f, springAlpha);

        return deltaMag * mult;
    }
    public float GetLaunchAlpha()
    {
        return launchControlAlpha;
    }

    public Vector2 GetLocalPos()
    {
        return localPos;
    }

    public void SetLocalPos(Vector2 inLocalPos)
    {
        Debug.Log("Software Cursor Local: " + localPos);
        localPos = inLocalPos;
        localPos = batteryController.weldState == BatteryController.WeldState.Welded ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);
        transform.position = (Vector2)parentForPos.transform.position + localPos;
    }

    public IEnumerator WeldJustStarted(float duration)
    {
        // Get correct angle to target
        Quaternion negClampQuat = Quaternion.AngleAxis((-batteryController.weldAngleClamp * Mathf.Rad2Deg) + 1f, Vector3.forward);
        Quaternion posClampQuat = Quaternion.AngleAxis((batteryController.weldAngleClamp * Mathf.Rad2Deg) - 1f, Vector3.forward);
        Vector2 negClampAngle = negClampQuat * -batteryController.weldSurfaceNormal;
        Vector2 posClampAngle = posClampQuat * -batteryController.weldSurfaceNormal;

        float posAngleDif = Vector2.Angle(posClampAngle, -parentForPos.transform.up);//(float)System.Math.Round(Mathf.Atan2(posClampAngle.y, posClampAngle.x) - Mathf.Atan2(parentForPos.transform.up.y, parentForPos.transform.up.x), 2);
        float negAngleDif = Vector2.Angle(negClampAngle, -parentForPos.transform.up);//(float)System.Math.Round(Mathf.Atan2(negClampAngle.y, negClampAngle.x) - Mathf.Atan2(parentForPos.transform.up.y, parentForPos.transform.up.x), 2);

        //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)posClampAngle * 2f), Color.red, 2f);
        //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)negClampAngle * 2f), Color.blue, 2f);
        //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + (parentForPos.transform.up * 2f), Color.yellow, 2f);

        //Debug.Log("Pos Dif: " + posAngleDif + " | " + "Neg Dif: " + negAngleDif);
        // Higher value == correct angle to use.
        if (posAngleDif > negAngleDif)
        {
            // Negative is target
            targetQuat = Quaternion.FromToRotation((Vector3)negClampAngle, -batteryController.weldSurfaceNormal);
        }
        else
        {
            // Positive is target
            targetQuat = Quaternion.FromToRotation((Vector3)posClampAngle, -batteryController.weldSurfaceNormal);
        }


        /// Fired from Battery Controller. This stops delta logic while player is snapping within range of weld.
        //weldInitialQuat = Quaternion.LookRotation(-parentForPos.transform.up);
        //weldInitialQuat = Quaternion.FromToRotation(-(Vector3)batteryController.weldSurfaceNormal, parentForPos.transform.up);
        //targetQuat = Quaternion.LookRotation(-(Vector3)batteryController.weldSurfaceNormal);
        //targetQuat = Quaternion.FromToRotation(-(Vector3)batteryController.weldSurfaceNormal, -(Vector3)batteryController.weldSurfaceNormal);
        correctWeldAlpha = 0f;
        justWelded = true;
        for (int i = 0; i < 10; i++)
        {
            correctWeldAlpha += (1f / 10f);
            yield return new WaitForSeconds(duration / 10f);
        }
        justWelded = false;
        yield break;
    }
}
