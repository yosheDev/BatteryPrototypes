using UnityEngine;
using Magnet;

public class a1_r3_events : MonoBehaviour, IInterfaceEvent
{
    [SerializeField] private GameObject radioGate;
    [SerializeField] private AudioSource sfxRadioStatic;
    public void InterfaceEvent(string eventName)
    {
        switch (eventName)
        {
            case "EnableRadio":
                GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.UIOnly);
                DialogueManager.instance.onDialogueEnded += OnMessagePlayFinished;
                DialogueManager.instance.BeginDialogue("a1_r3_radio");
                break;
        }
    }

    public void OnMessagePlayFinished()
    {
        DialogueManager.instance.onDialogueEnded -= OnMessagePlayFinished;
        GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Enabled);

        radioGate.GetComponent<AnimatorEvent>().InterfaceEvent("Activate");
        sfxRadioStatic.Stop();
    }
}
