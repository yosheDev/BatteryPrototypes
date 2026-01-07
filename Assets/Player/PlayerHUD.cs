using UnityEngine;
using UnityEngine.UI;
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Battery playerBattery;

    private void Awake()
    {
        playerBattery.onPercentChanged += UpdateSlider;
    }

    // Listen for player battery value change.
    private void UpdateSlider()
    {
        batterySlider.value = (int)playerBattery.GetPercent();
    }
}
