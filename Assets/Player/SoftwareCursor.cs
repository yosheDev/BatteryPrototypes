using UnityEngine;
using FunctionLibrary;
using System.Collections;

public class SoftwareCursor : MonoBehaviour
{
    [SerializeField] private BatteryController batteryController;
    private Vector2 localPos;
    private Vector2 launchControlPos; /// Used only for launch controls.
    private Vector3 worldPos = new Vector3(0f, 0f, 0f);
    public GameObject parentForPos;
    private Vector3 parentLastPos;
	private Vector2 aimDir;
	private Vector3 launchControlMin;
    private Vector3 launchControlMax;
    private float launchControlAlpha;
    private float playerSpriteLength = 1.5f;
    private bool justWelded = false;
    private Quaternion weldInitialQuat;
    private Quaternion targetQuat;
    private float correctWeldAlpha = 0f;
    private bool rotTest = false;
    public GameObject visObjTest;
    public Quaternion surfaceParentRotAdjust;

    void Update()
    {
        Vector2 lastFrameLocalPos = localPos;

        if (batteryController.GetParentSource() == null)
        {
            surfaceParentRotAdjust = Quaternion.identity;
        }
        else
        {
            //Debug.Log("RotQuat: " + surfaceParentRotAdjust.eulerAngles);
            //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((surfaceParentRotAdjust * Vector3.up) * 2f), Color.red, .1f);
        }
            Vector3 parentPosDelta = parentForPos.transform.position - parentLastPos;

        if (rotTest)
        {
            return;
        }

        //Debug.Log(localPos);
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
        float angleFromNormal = Vector3.SignedAngle((Vector3)batteryController.weldSurfaceNormal, (Vector3)aimDir, Vector3.forward);

        Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)aimDir * 1.5f), Color.blue, .1f);

        if (((batteryController.weldState == BatteryController.WeldState.Welded && (angleFromNormal > batteryController.weldAngleClamp || angleFromNormal < -batteryController.weldAngleClamp))))
        {
            // Clamp localPos back within range.
            if (justWelded)
            {

                #region Interp To Clamped Range Method
                Quaternion curRot = Quaternion.Slerp(weldInitialQuat, targetQuat, correctWeldAlpha);
                Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + (curRot * (batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.yellowGreen, 2f);
                //Debug.Log("Alpha: " + correctWeldAlpha);
                Vector2 cursorAimDir = (parentForPos.transform.position - (parentForPos.transform.position + (curRot * (batteryController.weldSurfaceNormal)))).normalized;

                transform.position = parentForPos.transform.position + (Vector3)(cursorAimDir * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f));

                // Things break when localPos stuff is uncommented. Related to aimDir im sure.
                localPos = transform.position - parentForPos.transform.position;
                localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
                #endregion

                #region Teleport Within Clamped Range Method
                /// This method is kept commented in case the simple math needs looked at. Interp version is better and is basically just this but with the slerp.
                //Vector2 cursorAimDir = (parentForPos.transform.position - (parentForPos.transform.position + (targetQuat * (batteryController.weldSurfaceNormal)))).normalized;
                //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + (targetQuat * (batteryController.weldSurfaceNormal) * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.darkMagenta, 2f);

                //// Handle transforms
                //transform.position = parentForPos.transform.position + (Vector3)(cursorAimDir * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f));
                ////Debug.DrawLine(parentForPos.transform.position, (Vector2)parentForPos.transform.position + (cursorAimDir * (parentForPos == batteryController.positiveMag.gameObject ? -4f : 2.5f)), Color.yellowNice, 2f);
                //localPos = transform.position - parentForPos.transform.position;
                //localPos = (batteryController.weldState == BatteryController.WeldState.Welded) ? (parentForPos == batteryController.positiveMag.gameObject ? ClampMagnitudeRange(localPos, 2.5f + playerSpriteLength, 2.45f + playerSpriteLength) : ClampMagnitudeRange(localPos, 2.5f, 2.45f)) : Vector2.ClampMagnitude(localPos, 2f);
                #endregion
            }
            else
            {
                localPos = lastFrameLocalPos;
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
                // This is gonna be overriding the rotate around that is added when parented.

                //Debug.Log("Normal Controls");
                worldPos = (Vector2)parentForPos.transform.position + localPos;
                transform.position = worldPos;
            }  
        }
        #endregion

        parentLastPos = parentForPos.transform.position;
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

    public void SetLocalPosFromWorld(Vector2 inWorldPos)
    {
        //localPos = parentForPos.transform.InverseTransformPoint(inWorldPos);
        //localPos = batteryController.weldState == BatteryController.WeldState.Welded ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);
        //Debug.DrawLine(parentForPos.transform.position + (Vector3)localPos, (parentForPos.transform.position + (Vector3)localPos) + new Vector3(0f, .5f, 0f), Color.orange, 1f);
        //transform.position = (Vector2)parentForPos.transform.position + localPos;
    }

    public void RotateLocalPos(Vector3 pivot, float angle)
    {
        // Either angle needs to change to be angle dif between its parent
        // OR function needs to change to use the correct transform and pivot points for rotation.

        // Okay how about instead I use the angle to just rotate the cursor around its own parentForPos. -> Can't, because the angle needed for that would be different.
        if (angle == 0f)
        {
            return;
        }
        surfaceParentRotAdjust = surfaceParentRotAdjust * Quaternion.AngleAxis(angle, Vector3.forward);
        //Debug.Log("Angle: " + angle);
        //Time.timeScale = .05f;
        // My manual set tests are wonky because it is not a child of the player. DUH. Should still inherit position from player, lets do that via delta method.
        //Vector3 parentPosDelta = parentForPos.transform.position - parentLastPos;

        // For some reason, these debug logs do NOT return the same thing...
        //Debug.Log(FunctionLibraryF.InverseTransformPointUnscaled(parentForPos.transform, transform.position));
        localPos = -1f * (parentForPos.transform.position - transform.position);
        //Debug.Log("L: " + localPos);
        localPos = batteryController.weldState == BatteryController.WeldState.Welded ? ClampMagnitudeRange(localPos, 3f, 2.95f) : Vector2.ClampMagnitude(localPos, 2f);

        transform.position = (Vector2)parentForPos.transform.position + localPos;
        transform.RotateAround(pivot, Vector3.forward, angle);

        visObjTest.transform.position = (Vector2)parentForPos.transform.position + localPos;
        visObjTest.transform.RotateAround(pivot, Vector3.forward, angle);
        
        //if (batteryController.playerWeldMag != null)
        //{
        //    Debug.DrawLine((Vector2)batteryController.playerWeldMag.transform.position, (Vector2)batteryController.playerWeldMag.transform.position + (batteryController.weldSurfaceNormal * 2f), Color.cornflowerBlue, .1f);
        //} 

        //rotTest = true;
        return;
    }

    public void SetNewPosParent(GameObject parent)
    {
        parentForPos = parent;
        parentLastPos = parentForPos.transform.position;
    }
    public IEnumerator WeldJustStarted(float duration)
    {
        // Get initial quat.
        weldInitialQuat = Quaternion.FromToRotation(-(Vector3)batteryController.weldSurfaceNormal, parentForPos.transform.up);

        // Get correct angle to target
        Quaternion negClampQuat = Quaternion.AngleAxis((-batteryController.weldAngleClamp * Mathf.Rad2Deg) + 1f, Vector3.forward);
        Quaternion posClampQuat = Quaternion.AngleAxis((batteryController.weldAngleClamp * Mathf.Rad2Deg) - 1f, Vector3.forward);
        Vector2 negClampAngle = negClampQuat * -batteryController.weldSurfaceNormal;
        Vector2 posClampAngle = posClampQuat * -batteryController.weldSurfaceNormal;

        float posAngleDif = Vector2.Angle(posClampAngle, -parentForPos.transform.up);
        float negAngleDif = Vector2.Angle(negClampAngle, -parentForPos.transform.up);

        //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)posClampAngle * 1f), Color.red, 2f);
        //Debug.DrawLine(parentForPos.transform.position, parentForPos.transform.position + ((Vector3)negClampAngle * 1f), Color.blue, 2f);
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

        // Begin timer.
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
