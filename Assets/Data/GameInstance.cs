using UnityEngine;

public class GameInstance : MonoBehaviour
{
    public bool initialEditorLoad = true;
    public bool loadingIntoArea = false;
    public bool isGamePaused = false;
    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hardcore
    }

    // NOTES:
    // I believe the goal for this class will be to keep data that could be wanted at any given time in any scene. This class will likely communicate directly with save/load system and data and cache certain parts of it when needed.
    // Will likely call save game / load game from this script.
    // This script could also possible keep track of application data if not already tracked by Unity.

    [Header("Game Data")]
    public GameDifficulty difficulty;

    [Header("Player Data")]
    public byte playerLives = 5;
    public byte maxPlayerLives = 5;
    public byte playerAbilityProgression = 0;
    public bool playerInputIgnored = false;

    public delegate void OnPlayerLivesChanged();
    public event OnPlayerLivesChanged onPlayerLivesChanged;

    public delegate void OnPlayerAbilityProgressChange();
    public event OnPlayerLivesChanged onPlayerAbilityProgressChange;

    #region Room Start Record
    public byte roomStartBattery = 100;
    #endregion

    public void UpdatePlayerAbilityProgression(byte newProgression)
    {
        playerAbilityProgression = newProgression;
        onPlayerAbilityProgressChange?.Invoke();
    }

    public void SetPlayerInputMode(BatteryController.PlayerInputMode mode)
    {
        GameObject.FindAnyObjectByType<BatteryController>().inputMode = mode;
    }

    #region Singleton
    public static GameInstance instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
}

    public void ResetPlayerLives()
    {
        playerLives = maxPlayerLives;
        onPlayerLivesChanged?.Invoke();
    }

    public void SetPlayerLives(byte amount)
    {
        playerLives = amount;
        instance.onPlayerLivesChanged?.Invoke();
    }

    public void SetMaxPlayerLives(byte amount)
    {
        maxPlayerLives = amount;
    }
    #endregion
}
