using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Room Data")]
    public bool isCheckpointRoom = false;

    [TextArea]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Respawn Pos will be set to location of this object!";

    [SerializeField] private Vector2 respawnPos; // This should be different than PlayerStart position is most cases. Think "metroid save room" where checkpoint device is not where player enters the room.

    private void Start()
    {
        respawnPos = (Vector2)gameObject.transform.position;
        if (isCheckpointRoom)
        {
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
