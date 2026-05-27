using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Magnet;
using PixeLadder.EasyTransition;

public class RoomManager : MonoBehaviour, IInterfaceEvent
{
    [Header("Room Data")]
    public bool isCheckpointRoom = false;
    public bool doesResetFullyReloadLevel = true;

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Respawn Pos will be set to location of this object!";

    [SerializeField] private Vector2 respawnPos; // This should be different than PlayerStart position is most cases. Think "metroid save room" where checkpoint device is not where player enters the room.
    public SpawningMechanism spawnMechanism;
    public TransitionEffect exitTransition;

    private void Start()
    {
        if (spawnMechanism == null)
        {
            spawnMechanism = GameObject.FindAnyObjectByType<SpawningMechanism>();
        }
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

        GameInstance.instance.roomStartBattery = GameObject.FindAnyObjectByType<BatteryController>().battery.GetPercent();
        respawnPos = (Vector2)gameObject.transform.position;
        if (isCheckpointRoom)
        {
            // This NEEDS to be called a bit after Start() so AreaManager can finish its editor only logic to update the room number correctly.
            UpdateCheckpoint(respawnPos);
        }
    }
    public void UpdateCheckpoint(Vector2 respawnPos)
    {
        /// This event is called at Start() if this room is a checkpoint room. This event can also be called from anything else.
        
        // figure out which one of these I will use and use only that.
        AreaManager.instance.UpdateCheckpointData(AreaManager.instance.GetCurrentRoom(), respawnPos);
    }

    public void InterfaceEvent(string name)
    {
        switch(name)
        {
            case "SetPlayerInputDisabled":
                GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Disabled);
                break;
            case "SetPlayerInputUIOnly":
                GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.UIOnly);
                break;
            case "SetPlayerInputScene":
                GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Scene);
                break;
            case "SetPlayerInputEnabled":
                GameInstance.instance.SetPlayerInputMode(BatteryController.PlayerInputMode.Enabled);
                break;
            default:
                break;
        }
    }
}
