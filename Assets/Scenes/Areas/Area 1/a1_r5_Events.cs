using Magnet;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class a1_r5_Events : MonoBehaviour, IInterfaceEvent
{
    public GameObject tabInteractObj;
    public GameObject cameraPanFocusObj;
    public GameObject gate;

    private void Start()
    {
        // If player died and is respawning here.
        if (AreaManager.instance.checkpointRespawnCount == 1)
        {
            gate.GetComponent<IInterfaceEvent>().InterfaceEvent("Activate");

            // Force new dialogue sequence.
        }
        else if (AreaManager.instance.checkpointRespawnCount > 1)
        {
            gate.GetComponent<IInterfaceEvent>().InterfaceEvent("Activate");
        }
        // First time in the room.
        else
        {
            tabInteractObj.SetActive(false);
        } 
    }
    public void InterfaceEvent(string name)
    {
        switch(name)
        {
            case "EnableTab":
                StartCoroutine(EnableTabulatorSequence());
                break;
            default:
                break;
        }
    }

    private IEnumerator EnableTabulatorSequence()
    {
        Transform playerSpriteTransform = GameObject.FindAnyObjectByType<BatteryController>().transform;

        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Scene);

        Debug.Log("Pan camera to the point.");
        CameraManager.instance.AddFollowTarget(cameraPanFocusObj.transform, 1f);
        CameraManager.instance.RemoveFollowTarget(playerSpriteTransform, 1f);
        yield return new WaitForSeconds(6f);
        Debug.Log("Pan camera back to player.");
        CameraManager.instance.AddFollowTarget(playerSpriteTransform, 1f);
        CameraManager.instance.RemoveFollowTarget(cameraPanFocusObj.transform, 1f);
        yield return new WaitForSeconds(2f);

        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Enabled);
        gate.GetComponent<IInterfaceEvent>().InterfaceEvent("Activate");

        tabInteractObj.SetActive(true);
        yield break;
    }
}
