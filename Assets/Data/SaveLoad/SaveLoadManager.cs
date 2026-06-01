using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private string saveFilePath;
    public SaveGameData currentSettings = new SaveGameData();

    public static SaveLoadManager instance;

    private void Awake()
    {
        // Establishes path targeting a safe, writable local directory
        saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
        LoadSettings();

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

    public void SaveSettings()
    {
        try
        {
            // Convert C# object data down to raw JSON string formatting
            string jsonString = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(saveFilePath, jsonString);
            Debug.Log("Settings saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save settings file: {e.Message}");
        }
    }

    public void LoadSettings()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string jsonString = File.ReadAllText(saveFilePath);

                JsonUtility.FromJsonOverwrite(jsonString, currentSettings);
                Debug.Log("Settings loaded successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Settings file corrupted. Loading defaults: {e.Message}");
                currentSettings = new SaveGameData();
            }
        }
        else
        {
            // Initializes standard settings if no previous file matches pathing
            currentSettings = new SaveGameData();
            SaveSettings();
        }
    }
}