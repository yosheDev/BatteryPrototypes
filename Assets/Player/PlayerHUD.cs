using NUnit;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Canvas hudCanvas;
    [HideInInspector] public CanvasGroup hudGroup;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Battery playerBattery;
    [SerializeField] private TextMeshProUGUI playerLivesText;
    [SerializeField] private TextMeshProUGUI abilityProgressText;
    [SerializeField] private TextMeshProUGUI spawnHintText;

    private Coroutine fadeHUDRoutine;

    private void Awake()
    {
        playerBattery.onPercentChanged += UpdateSlider;
        hudGroup = hudCanvas.GetComponent<CanvasGroup>();
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

    public void SetDisplaySpawnText(bool visible)
    {
        spawnHintText.gameObject.SetActive(visible);
    }

    public void FadeHUD(float opacity, float duration = .5f)
    {
        opacity = Mathf.Clamp01(opacity);
        duration = Mathf.Clamp(duration, 0f, float.MaxValue);

        // If instantaeneos
        if (duration <= 0f)
        {
            hudGroup.alpha = opacity;
            return;
        }

        if (fadeHUDRoutine != null)
        {
            StopCoroutine(fadeHUDRoutine);
        }

        fadeHUDRoutine = StartCoroutine(FadeHUDTimer(opacity, duration));
    }

    private IEnumerator FadeHUDTimer(float opacity, float duration)
    {
        float start = hudGroup.alpha;
        float end = opacity;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hudGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        hudGroup.alpha = end;

        yield break;
    }
}
