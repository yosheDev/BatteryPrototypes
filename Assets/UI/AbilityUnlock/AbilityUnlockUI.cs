using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
public class AbilityUnlockUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityDescription;
    [SerializeField] private Image abilityIcon;

    public void DisplayAbilityUnlock(byte abilityID, float alpha, float duration = 1f)
    {
        switch (abilityID)
        {
            case 1:
                abilityName.text = "Polarity Module";
                abilityDescription.text = "Electromagnets implemented into chassis. Electromagnetic forces will propel you forwards in magnetic fields.";
                break;
            case 2:
                abilityName.text = "Ferro Anchor";
                abilityDescription.text = "Weld onto magnetic surfaces with SPACE. Anchor in place, or swing to move laterally along the surface.";
                break;
        }

        StartCoroutine(BlendCanvasGroup(abilityID, alpha, duration));
    }

    private IEnumerator BlendCanvasGroup(byte abilityID, float targetValue, float duration = 1f)
    {
        float elapsedTime = 0f;
        float startValue = canvasGroup.alpha;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }

        yield break;
    }
}
