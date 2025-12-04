using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaManager : MonoBehaviour
{
    public Areas area;

    private int roomNum = 1;

    #region Singleton
    public static AreaManager instance;

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
    #endregion
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Level startLevel = new Level(area, 1);
        SceneManagement.LoadScene(startLevel);
    }

    public void LoadNextRoom()
    {
        roomNum++;
        Level nextRoom = new Level(area, roomNum);

        if (SceneManagement.DoesSceneExist(nextRoom))
        {
            Debug.Log("Loading Next Room: " + nextRoom.area + " " + nextRoom.room);
            SceneManagement.LoadScene(nextRoom);
        }
        else
        {
            Debug.LogError("Scene does not exist.");
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // TO DO: Make this end transition and begin level from players perspective.
    }
}
