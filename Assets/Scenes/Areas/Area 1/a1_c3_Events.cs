using Magnet;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class a1_c3_Events : MonoBehaviour, IInterfaceEvent
{
    public GameObject tabEnableObj;
    public GameObject tabInteractObj;
    public GameObject tabulator;
    public GameObject cameraPanFocusObj;
    public GameObject gate;

    private void Start()
    {
        // If player died and is respawning here.
        if (AreaManager.instance.checkpointRespawnCount == 1)
        {
            gate.GetComponent<IInterfaceEvent>().InterfaceEvent("Activate");
            tabEnableObj.SetActive(false);
            tabInteractObj.SetActive(false);

            // Force dialogue sequence.
            AreaManager.instance.ReleasePlayer();
            tabulator.GetComponent<tabulator>().SetDialogueCSV("a1_c3_1");
            DialogueManager.instance.onDialogueEnded += SetFinalDialogue;
            tabulator.GetComponent<tabulator>().StartDialogueScene();
        }
        else if (AreaManager.instance.checkpointRespawnCount > 1)
        {
            gate.GetComponent<IInterfaceEvent>().InterfaceEvent("Activate");
            tabulator.GetComponent<tabulator>().SetDialogueCSV("a1_c3_2");
            tabEnableObj.SetActive(false);
            tabInteractObj.SetActive(true);
        }
        // First time in the room.
        else
        {
            tabInteractObj.SetActive(false);
        }
    }

    public void SetFinalDialogue()
    {
        DialogueManager.instance.onDialogueEnded -= SetFinalDialogue;
        tabulator.GetComponent<tabulator>().SetDialogueCSV("a1_c3_2");
        tabInteractObj.SetActive(true);
    }

    public void InterfaceEvent(string name)
    {
        switch (name)
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
        tabEnableObj.SetActive(false);

        Debug.Log("Pan camera to the point.");
        CameraManager.instance.AddFollowTarget(cameraPanFocusObj.transform, 1f);
        CameraManager.instance.RemoveFollowTarget(playerSpriteTransform, 1f);

        yield return new WaitForSeconds(6f);
        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.UIOnly);
        DialogueManager.instance.onDialogueEnded += TabFirstAwakenEnded;
        DialogueManager.instance.BeginDialogue("a1_c3_1");

        yield break;
    }

    public void TabFirstAwakenEnded()
    {
        DialogueManager.instance.onDialogueEnded -= TabFirstAwakenEnded;
        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Scene);
        StartCoroutine(FinishEnableTabulatorSequence());
    }

    private IEnumerator FinishEnableTabulatorSequence()
    {
        Transform playerSpriteTransform = GameObject.FindAnyObjectByType<BatteryController>().transform;

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
