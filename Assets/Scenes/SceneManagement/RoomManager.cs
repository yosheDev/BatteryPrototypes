using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Room Data")]
    public bool isCheckpointRoom = false;
    public bool doesResetFullyReloadLevel = true;

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Respawn Pos will be set to location of this object!";

    [SerializeField] private Vector2 respawnPos; // This should be different than PlayerStart position is most cases. Think "metroid save room" where checkpoint device is not where player enters the room.
    public SpawningMechanism spawnMechanism;

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForEndOfFrame();

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
}
