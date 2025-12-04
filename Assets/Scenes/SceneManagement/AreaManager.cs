using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaManager : MonoBehaviour
{
    public Areas area;
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Level startLevel = new Level(area, 1);
        SceneManagement.LoadScene(startLevel);
        Debug.Log("Loaded start level.");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene has been loaded.");
    }
}
