using UnityEngine;
using UnityEngine.InputSystem;
using static BatteryController;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class PauseMenu : MonoBehaviour
{
    public Canvas pauseCanvas;
    public GameObject pauseMenuObj;
    public GameObject settingsMenuObj;

    #region Singleton
    public static PauseMenu instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public void PauseInput(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        if (!GameInstance.instance.isGamePaused)
        {
            PauseGame();
        }
        else
        {
            UnpauseGame();
        }
    }

    public void PauseGame()
    {
        GameInstance.instance.isGamePaused = true;

        pauseCanvas.gameObject.SetActive(true);
        pauseMenuObj.SetActive(true);
        settingsMenuObj.SetActive(false);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnpauseGame()
    {
        GameInstance.instance.isGamePaused = false;

        pauseCanvas.gameObject.SetActive(false);
        pauseMenuObj.SetActive(false);
        settingsMenuObj.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        settingsMenuObj.GetComponent<SettingsMenu>().InitializeSettingsMenu();

        pauseMenuObj.SetActive(false);
        settingsMenuObj.SetActive(true);
    }

    public void ReturnToMenu()
    {
        pauseMenuObj.SetActive(true);
        settingsMenuObj.SetActive(false);
    }
}
