using Magnet;
using System.Collections;
using UnityEngine;

public class tabulator : MonoBehaviour, IInterfaceEvent
{
    [SerializeField] private Transform camFocusTrans;
    [SerializeField] private string currentDialogueData = "dialogueTest";

    #region Dialogue
    public void SetDialogueCSV(string csvName)
    {
        currentDialogueData = csvName;
    }
    public void StartDialogueScene()
    {
        StartCoroutine(DialogueSceneIntro());
    }

    public IEnumerator DialogueSceneIntro()
    {
        Transform playerSpriteTransform = GameObject.FindAnyObjectByType<BatteryController>().transform;

        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.UIOnly);

        Debug.Log("Pan camera to the point.");
        CameraManager.instance.AddFollowTarget(camFocusTrans, 1f);
        CameraManager.instance.RemoveFollowTarget(playerSpriteTransform, 1f);

        yield return new WaitForSeconds(1f);
        StartDialogue();
        yield break;
    }

    public void StartDialogue()
    {
        DialogueManager.instance.onDialogueEnded += EndDialogue;
        DialogueManager.instance.BeginDialogue(currentDialogueData);
    }

    public void EndDialogue()
    {
        DialogueManager.instance.onDialogueEnded -= EndDialogue;
        StartCoroutine(DialogueSceneOutro());
    }
    public IEnumerator DialogueSceneOutro()
    {
        Transform playerSpriteTransform = GameObject.FindAnyObjectByType<BatteryController>().transform;

        Debug.Log("Pan camera back to player.");
        CameraManager.instance.AddFollowTarget(playerSpriteTransform, 1f);
        CameraManager.instance.RemoveFollowTarget(camFocusTrans, 1f);
        yield return new WaitForSeconds(2f);

        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Enabled);
        yield break;
    }

    #endregion

    public void InterfaceEvent(string eventName)
    {
        switch (eventName)
        {
            case "StartDialogue":
                StartDialogueScene();
                break;
            default:
                break;
        }
    }
}
