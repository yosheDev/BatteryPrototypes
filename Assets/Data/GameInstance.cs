using UnityEngine;

public class GameInstance : MonoBehaviour
{
    // NOTES:
    // I believe the goal for this class will be to keep data that could be wanted at any given time in any scene. This class will likely communicate directly with save/load system and data and cache certain parts of it when needed.
    // Will likely call save game / load game from this script.
    // This script could also possible keep track of application data if not already tracked by Unity.

    [Header("Player Data")]
    public byte playerLives = 5;

    #region Singleton
    public static GameInstance instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetPlayerLives()
    {
        playerLives = 5;
    }
    #endregion
}
