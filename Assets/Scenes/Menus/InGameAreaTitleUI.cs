using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameAreaTitleUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private TextMeshProUGUI chapterText;
    [SerializeField] private TextMeshProUGUI areaText;

    #region Singleton
    public static InGameAreaTitleUI instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public void DisplayAreaTitle(Areas area, int roomNum)
    {
        chapterText.text = "Chapter ";
        chapterText.text += (int)area;

        switch (area)
        {
            case Areas.Area0:
                areaText.text = "Testing Grounds";
                break;
            case Areas.Area1:
                switch(roomNum)
                {
                    case 4:
                        chapterText.text = "Chapter 1";
                        areaText.text = "The Nest";
                        break;
                    case 11:
                        chapterText.text = "Chapter 1-2";
                        areaText.text = "The Climb";
                        break;
                    case 19:
                        chapterText.text = "Chapter 1-3";
                        areaText.text = "The Upper Nest";
                        break;
                    case 26:
                        chapterText.text = "Chapter 1-4";
                        areaText.text = "The Plunge";
                        break;
                    default:
                        chapterText.text = "Chapter 1";
                        areaText.text = "The Nest";
                        break;
                }
                
                break;
            case Areas.Area2:
                areaText.text = "Facility";
                break;
            case Areas.Area3:
                areaText.text = "The Sect";
                break;
            case Areas.Area4:
                areaText.text = "Downed";
                break;
            case Areas.Area5:
                areaText.text = "Colony";
                break;
            case Areas.Area6:
                areaText.text = "Eden";
                break;
        }

        StartCoroutine(DisplayAreaTimer());
    }

    private IEnumerator DisplayAreaTimer()
    {
        yield return StartCoroutine(FadeInGroup());

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeOutGroup());

        yield break;
    }

    private IEnumerator FadeOutGroup()
    {
        for (int i = 0; i <= 60f; i++)
        {
            titleGroup.alpha = Mathf.Clamp01(1f - (i * .0166f));
            yield return new WaitForSeconds(.05f);
        }
        titleGroup.alpha = 0f;
        yield break;
    }

    private IEnumerator FadeInGroup()
    {
        for (int i = 0; i <= 40f; i++)
        {
            titleGroup.alpha = Mathf.Clamp01(0f + (i * .025f));
            yield return new WaitForSeconds(.05f);
        }

        titleGroup.alpha = 1f;
        yield break;
    }
}
