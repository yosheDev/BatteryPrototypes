using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningIndicatorsGroupActivator : MonoBehaviour
{
    [SerializeField] private List<WarningIndicator> indicators = new List<WarningIndicator>();
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
}
