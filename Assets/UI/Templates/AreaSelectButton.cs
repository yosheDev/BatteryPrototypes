using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaSelectButton : MonoBehaviour
{
    [SerializeField] private Areas area;
    [SerializeField] private TMP_Text _text;

    // Initialize calls when factory creates this.
    public void Initialize(Areas newArea)
    {
        area = newArea;
        _text.SetText(SceneManagement.GetAreaDisplayName(area));
    }

    public void EnterArea()
    {
        // Bind delegate when area scene is loaded. Has to be done this way because the area select is not unloading until another scene is loaded.
        SceneManager.sceneLoaded += UnloadAreaSelect;

        Level newArea = new Level(area, -1);
        if (SceneManagement.DoesSceneExist(newArea))
        {
            SceneManagement.LoadScene(newArea);
        }
        else
        {
            Debug.LogError("Level " + SceneManagement.GetSceneFormattedName(newArea) + "(" + newArea.area + " " + newArea.room + ") does not exist. Unable to load from AreaSelectButton.cs");
        }
    }

    private void UnloadAreaSelect(Scene scene, LoadSceneMode mode)
    {
        SceneManagement.UnloadSceneAsync("AreaSelection");
        SceneManager.sceneLoaded -= UnloadAreaSelect;
    }
}
