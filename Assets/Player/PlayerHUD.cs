using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Battery playerBattery;
    [SerializeField] private TextMeshProUGUI playerLivesText;

    private void Awake()
    {
        playerBattery.onPercentChanged += UpdateSlider;
        
    }
    private void Start()
    {
        if (GameInstance.instance.difficulty == GameInstance.GameDifficulty.Easy)
        {
            playerLivesText.gameObject.SetActive(false);
        }
        else
        {
            GameInstance.instance.onPlayerLivesChanged += UpdatePlayerLives;
            playerLivesText.SetText(GameInstance.instance.playerLives.ToString());
        }
    }

    // Listen for player battery value change.
    private void UpdateSlider()
    {
        batterySlider.value = (int)playerBattery.GetPercent();
    }

    private void UpdatePlayerLives()
    {
        playerLivesText.SetText(GameInstance.instance.playerLives.ToString());
    }
}
