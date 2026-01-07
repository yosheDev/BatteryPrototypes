#nullable enable
using System.Collections.Generic;
using System.Text;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.SceneManagement;

#region Universal Data
public enum Areas
{
    // Just need to add or rename areas here when they exist.
    None = -1,
    Area0 = 0,
    Area1 = 1,
    Area2,
    Area3
}
public struct Level
{
    public Areas area;
    public int room;

    public Level(Areas a, int r)
    {
        area = a;
        room = r;
    }
}
#endregion
public static class SceneManagement
{
    public static Dictionary<Areas, string> areaDisplayNameData = new Dictionary<Areas, string>();
  
    #region Load / Unload Scene
    public static void LoadScene(Level level, string? overrideString = null, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
    {
        string roomSceneName;

        if (overrideString != null)
        {
            roomSceneName = overrideString;
        }
        else
        {
            roomSceneName = GetSceneFormattedName(level);
        }
        
        SceneManager.LoadScene(roomSceneName, loadSceneMode);
    }

    public static void UnloadSceneAsync(Level level, string? overrideString = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
    {
        string roomSceneName;

        if (overrideString != null)
        {
            roomSceneName = overrideString;
        }
        else
        {
            roomSceneName = GetSceneFormattedName(level);
        }

        SceneManager.UnloadSceneAsync(roomSceneName);
    }

    public static void UnloadSceneAsync(string? overrideString = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
    {
        SceneManager.UnloadSceneAsync(overrideString);
    }
    #endregion

    #region Utility
    [Tooltip("Gets the scene name for an area or room scene.")]
    public static string GetSceneFormattedName(Level level)
    {
        // This function gets the scene name for an area / room scene. If an override string is passed, this function is not called as that will be used instead.
        StringBuilder sb = new StringBuilder("", 7);

        sb.Append("a");             // Area
        sb.Append((int)level.area); // Area index

        // If there is no room.
        if (level.room != -1)
        {
            sb.Append("_r");             // spacer
            sb.Append(level.room);      // room index
        }

        return sb.ToString();
    }

    public static bool DoesSceneExist(string sceneName)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

        return (buildIndex != -1);
    }

    public static bool DoesSceneExist(Level level)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(GetSceneFormattedName(level));
        return (buildIndex != -1);
    }

    private static string SceneNameFromBuildIndex(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }

    #region Area Display Name
    public static string GetAreaDisplayName(Areas area)
    {
        /// If still need to retrieve the data.
        if (areaDisplayNameData.Count <= 0)
        {
            ParseAreaDisplayNameDictionary();
        }

        string output = "";
        areaDisplayNameData.TryGetValue(area, out output);
        return output;
    }

    ///  Move this to its own class at some point? Maybe not?
    private static void ParseAreaDisplayNameDictionary(bool skipHeader = true)
    {
        TextAsset csv = Resources.Load<TextAsset>("areaDisplayNames");

        // Split the csv into lines(rows).
        string[] lines = csv.text.Split('\n');

        // Loop through lines(rows) to seperate columns based on commas. (starting at 1 to skip header)
        for (int i = skipHeader ? 1 : 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // Split line into columns
            string[] columns = lines[i].Split(',');

            // Assuming CSV structure: Build Index, Name(English), 
            int buildIndex = int.Parse(columns[0]);
            string displayName = columns[1];

            areaDisplayNameData.Add((Areas)areaDisplayNameData.Count, displayName);
            Debug.Log("Loaded: " + (Areas)areaDisplayNameData.Count + " -> " + displayName);
        }
    }
    #endregion

    #endregion
}
