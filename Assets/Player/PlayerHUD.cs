using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Battery playerBattery;
    [SerializeField] private TextMeshProUGUI playerLivesText;
    [SerializeField] private TextMeshProUGUI abilityProgressText;

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

        GameInstance.instance.onPlayerAbilityProgressChange += UpdateAbilityText;
        GameInstance.instance.UpdatePlayerAbilityProgression(GameInstance.instance.playerAbilityProgression);
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

    public void UpdateAbilityText()
    {
        StringBuilder newText = new StringBuilder("Ability Progress: " + GameInstance.instance.playerAbilityProgression.ToString(), 21);
        abilityProgressText.text = newText.ToString();
    }
}
