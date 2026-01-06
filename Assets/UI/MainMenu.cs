using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // THIS WHOLE CLASS IS PLACEHOLDER. Don't take anything in here too seriously, it is all replaceable for better systems down the line.

    private bool newGame = false;

    private void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    //  TO DO: Pretty much all of this.
    public void NewGame()
    {
        newGame = true;
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    void OnSceneUnloaded(Scene scene)
    {
        //if (newGame)
        //{
            Level level = new Level(Areas.Area0, 1);
            SceneManagement.LoadScene(level);
        //}
    }
    public void LoadGame()
    {

    }
}
