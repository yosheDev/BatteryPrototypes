#nullable enable
using PixeLadder.EasyTransition;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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
    /// <summary>
    /// Load a scene based on the Level struct passed in. If an override string is passed, load that scene instead.
    /// </summary>
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

        //SceneManager.LoadScene(roomSceneName, loadSceneMode);
        try
        {
            SceneTransitioner.Instance.LoadScene(roomSceneName, AreaManager.instance.roomManager.exitTransition, loadSceneMode);
        }
        catch
        {
            SceneTransitioner.Instance.LoadScene(roomSceneName, SceneTransitioner.Instance.GetDefaultTransition(), loadSceneMode);
        }
    }

    public static void LoadScene(string? overrideString = null, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
    {
        /// This function forwards to the main LoadScene function with a dummy Level struct. For when loading scene with just the string for the name.
        Level dummyLevel = new Level(0, -1);
        SceneManagement.LoadScene(dummyLevel, overrideString, loadSceneMode);
    }

    /// <summary>
    /// Unload a scene based on the Level struct passed in. If an override string is passed, unload that scene instead.
    /// </summary>
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
        Debug.Log("Unloading: " + roomSceneName);
        try
        {
            SceneManager.UnloadSceneAsync(roomSceneName);
        }
        catch
        {
            Debug.LogError("Unable to unload " + roomSceneName + " as it was invalid. Has this scene already been unloaded? Is this the only currently loaded scene? " + SceneManager.sceneCount);
        }
        
    }

    /// <summary>
    /// Unload a scene based on the Level struct passed in. If an override string is passed, unload that scene instead.
    /// </summary>
    public static void UnloadSceneAsync(string? overrideString = null, UnloadSceneOptions unloadSceneOptions = UnloadSceneOptions.None)
    {
        /// This function forwards to the main UnloadScene function with a dummy Level struct. For when loading scene with just the string for the name.
        Level dummyLevel = new Level(0, -1);
        SceneManagement.UnloadSceneAsync(dummyLevel, overrideString, unloadSceneOptions);
    }
    #endregion

    #region Utility
    /// <summary>
    /// Given a level struct, returns the string of that scene. String is parsed based on the level struct, so scenes must be named correctly.
    /// </summary>
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

    /// <summary>
    /// Returns whether or not this scene is included in the build profile.
    /// </summary>
    public static bool DoesSceneExist(string sceneName)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);

        return (buildIndex != -1);
    }

    /// <summary>
    /// Returns whether or not this scene is included in the build profile.
    /// </summary>
    public static bool DoesSceneExist(Level level)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(GetSceneFormattedName(level));
        return (buildIndex != -1);
    }

    /// <summary>
    /// Returns whether or not the given scene is a room scene. Searches for "_r" in the scene.name.
    /// </summary>
    public static bool IsSceneARoom(Scene scene)
    {
        // May need to adjust this later, but for now since only the rooms have "_r" in the name(i.e a0_r1) use this confirm if this is a room.
        Debug.Log(scene.name + " is a room? -> " + scene.name.Contains("_r"));
        return scene.name.Contains("_r");
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
