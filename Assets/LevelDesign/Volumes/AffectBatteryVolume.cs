using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
///  NOTE: This should never have its collider have callbacks set for the Default channel. It is causing volume to sometimes block the player for some reason upon intial collisions. Just make new layer for other batteries.
/// </summary>
public class AffectBatteryVolume : MonoBehaviour
{
    private enum VolumeType
    {
        Charge,
        Drain
    }
    [SerializeField] private VolumeType affectType = VolumeType.Charge;
    [Tooltip("When true, only implements amount once when entering collider.")]
    [SerializeField] private bool onlyTriggerOnce = false;
    [Tooltip("The frequency that amount is implemented on the batteries within the volume.")]
    [SerializeField] private float frequency = 0.5f;
    [Tooltip("The amount that is implemented on the battery percentages within the volume.")]
    [SerializeField] private byte amount = 5;

    private IEnumerator affectCoroutine;

    /// Stores affected objects and their initial gravity values when first entering the volume. 
    private HashSet<Battery> affectedBatteries = new HashSet<Battery>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Battery foundBattery = null;
        if (collision.gameObject.GetComponent<Battery>() != null)
        {
            foundBattery = collision.gameObject.GetComponent<Battery>();

            if (onlyTriggerOnce)
            {
                Affect(foundBattery);
            }
            else
            {
                //Debug.Log("ENTER -> Found Battery: " + foundBattery + "   Collider: " + collision);
                affectedBatteries.Add(foundBattery);

                if (affectCoroutine == null)
                {
                    StartCoroutine(AffectBatteries());
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Battery foundBattery = null;
        if (collision.gameObject.GetComponent<Battery>() != null)
        {
            foundBattery = collision.gameObject.GetComponent<Battery>();
            //Debug.Log("EXIT -> Found Battery: " + foundBattery + "   Collider: " + collision);
            affectedBatteries.Remove(foundBattery);
        }  
    }

    private IEnumerator AffectBatteries()
    {
        while (affectedBatteries.Count > 0)
        {
            foreach (Battery battery in affectedBatteries)
            {
                //Debug.Log(battery);
                Affect(battery);
            }

            yield return new WaitForSeconds(frequency);
        }

        affectCoroutine = null;
    }

    private void Affect(Battery battery)
    {
        switch (affectType)
        {
            case VolumeType.Charge:
                battery.AddPercent(amount);
                break;

            case VolumeType.Drain:
                battery.SubtractPercent(amount);
                break;

            default:
                battery.AddPercent(amount);
                break;
        }
    }
}
