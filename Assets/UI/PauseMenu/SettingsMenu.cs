using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    SaveGameData initialSettings;
    SaveGameData newSettings;

    #region References
    public Slider mouseSensitivitySlider;
    public TextMeshProUGUI mouseSensitivityText;
    #endregion

    public void InitializeSettingsMenu()
    {
        initialSettings = SaveLoadManager.instance.currentSettings;
        newSettings = new SaveGameData();

        #region Update UI
        mouseSensitivitySlider.value = Mathf.RoundToInt(initialSettings.mouseSensitivity * 10f);
        mouseSensitivityText.text = initialSettings.mouseSensitivity.ToString("F1");
        #endregion
    }

    #region Update Settings
    public void UpdateMouseSensitivity(float sens)
    {
        newSettings.mouseSensitivity = sens / 10f;

        mouseSensitivityText.text = (sens / 10f).ToString("F1");
    }
     
    #endregion

    public void CancelSettings()
    {
        SaveLoadManager.instance.currentSettings = initialSettings;

        SaveLoadManager.instance.SaveSettings();

        PauseMenu.instance.ReturnToMenu();
    }

    public void ApplySettings()
    {
        Debug.Log("Apply Settings");
        SaveLoadManager.instance.currentSettings = newSettings;

        SaveLoadManager.instance.SaveSettings();

        PauseMenu.instance.ReturnToMenu();
    }
}
