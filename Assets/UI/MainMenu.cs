using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // THIS WHOLE CLASS IS PLACEHOLDER. Don't take anything in here too seriously, it is all replaceable for better systems down the line.

    private bool newGame = false;

    //  TO DO: Pretty much all of this.
    public void NewGame()
    {
        newGame = true;
        SceneManager.sceneLoaded += UnloadMainMenu;
        SceneManager.LoadScene("AreaSelection", LoadSceneMode.Additive); 
    }

    private void UnloadMainMenu(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);
        SceneManager.sceneLoaded -= UnloadMainMenu;
        SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void LoadGame()
    {

    }
}
