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
    private void Start()
    {
        Debug.Log(batterySlider);
        Debug.Log(playerBattery);
    }
    // Listen for player battery value change.
    private void UpdateSlider()
    {
        Debug.Log("Update Slider");
        batterySlider.value = (int)playerBattery.GetPercent();
    }
}
