using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningIndicatorsGroupActivator : MonoBehaviour
{
    [SerializeField] private List<ClampRenderersToScreen> indicators = new List<ClampRenderersToScreen>();
    //[SerializeField] private float duration = 4f;
    //[SerializeField] private float interval = .25f;
    //[SerializeField] private float flashDuration = .15f;

    public void FlashIndicators(float duration, float interval, float flashDuration)
    {
        for (int i = 0;  i < indicators.Count; i++)
        {
            indicators[i].FlashIndicator(duration, interval, flashDuration);
        }
    }

    public void StopFlashIndicators()
    {
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].StopFlashIndicator();
        }
    }
}
