using PixeLadder.EasyTransition;
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
        GameInstance.instance.loadingIntoArea = true;

        // Bind delegate when area scene is loaded. Has to be done this way because the area select is not unloading until another scene is loaded.
        //SceneManager.sceneLoaded += UnloadAreaSelect;

        Level newArea = new Level(area, -1);
        Level newAreaFirstRoom = new Level(area, 1);
        Level[] loadScenes = new Level[] { newArea, newAreaFirstRoom};
        string[] unloadSceneStrings = new string[] { "AreaSelection" };

        if (SceneManagement.DoesSceneExist(newArea))
        {
            SceneManagement.LoadScene(SceneTransitioner.SceneTransitionOrder.LoadUnload, null, loadScenes, unloadSceneStrings);
        }
        else
        {
            Debug.LogError("Level " + SceneManagement.GetSceneFormattedName(newArea) + "(" + newArea.area + " " + newArea.room + ") does not exist. Unable to load from AreaSelectButton.cs");
        }
    }
}
