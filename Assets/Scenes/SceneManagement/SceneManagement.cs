#nullable enable
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
  
    #region Load Scene
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
    #endregion

    #region Utility
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

    #endregion
}
