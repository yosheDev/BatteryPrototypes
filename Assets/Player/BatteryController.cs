using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using Magnet;

public class BatteryController : MonoBehaviour
{
    #region Properties
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cursorObj;
    [SerializeField] private MagneticPoint positiveMag;
    [SerializeField] private MagneticPoint negativeMag;

    [Header("Control Settings")]
    [SerializeField] private float rotationFactor;

    [HideInInspector] public Vector2 cursorPosWS;
    [HideInInspector] public Vector2 mouseDelta;
    //==============================================================================================================================
    #endregion 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        // Get Aim Target Vector
        Vector3 aimDir = (cursorObj.transform.position - transform.position).normalized;
        Quaternion targetAimQuat = Quaternion.LookRotation(Vector3.forward, aimDir);

        // Apply rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAimQuat, Time.deltaTime * rotationFactor);

        #region Apply Magnetic Forces
        // Get Overlapped Magneting Fields From Magnet Components 
        /// Handle + and - sides seperately, as those will apply forces at different points of player collider.

        // [Handle Positive Magnet]
        Vector2 combinedPositiveForces = Vector2.zero;

        List<MagnetComponentBase> positiveFields = positiveMag.affectFields.ToList();
        for (int i = 0; i < positiveFields.Count; i++)
        {
            // Will need to replace this with new function when I have that.
            combinedPositiveForces += positiveFields[i].GetAppliedForce(positiveMag._magData, positiveMag.transform.position, positiveMag._radius);
        }

        // [Handle Negative Magnet]
        Vector2 combinedNegativeForces = Vector2.zero;

        List<MagnetComponentBase> negativeFields = negativeMag.affectFields.ToList();
        for (int i = 0; i < negativeFields.Count; i++)
        {
            // Will need to replace this with new function when I have that.
            Debug.Log(negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._radius));
            combinedNegativeForces += negativeFields[i].GetAppliedForce(negativeMag._magData, negativeMag.transform.position, negativeMag._radius);
        }

        // Combine Forces and Apply
        //rb.AddForceAtPosition(combinedPositiveForces, positiveMag.transform.position);
        //Debug.Log("Positive Force: " + combinedPositiveForces);
        rb.AddForceAtPosition(combinedNegativeForces, negativeMag.transform.position);
        Debug.Log("Negative Force: " + combinedNegativeForces);

        #endregion
    }

    #region Input Actions
    public void UpdateMouseDelta(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>().magnitude < 50f ? (context.ReadValue<Vector2>()) : mouseDelta; /// Magnitude check protects against mouse connectivity errors.
    }

    // Currently unused.
    public void Tilt(InputAction.CallbackContext context)
    {
        if (playerInput.currentControlScheme.Equals("Gamepad"))
        {

        }
        else
        {
            //cursorPosWS = mainCam.ScreenToWorldPoint(context.ReadValue<Vector2>());
            
        }
    }

    #endregion
}
