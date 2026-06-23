using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RadioMessageBox : MonoBehaviour
{
    private TextMeshPro messageText;

    [SerializeField] private List<string> messageStrings;   /// Messages to cycle.
    [SerializeField] private float cycleDuration = 4f;           /// Length a message will stay before cycling.
    [SerializeField] private byte distortionLevel = 4;          /// Lower = less distortion.
    [SerializeField] private string startTag = "<link = ShakeVertical1 + ShakeHorizontal1>";
    [SerializeField] private string endTag = "</link>";
    private int messageIndex = 0;

    void Start()
    {
        messageText = GetComponent<TextMeshPro>();
        messageText.text = DistortMessage(messageStrings[messageIndex]);
        StartCoroutine(CycleMessages());
    }

    private void DisplayNextMessage()
    {
        messageIndex++;
        if (messageIndex >= messageStrings.Count)
        {
            messageIndex = 0;
        }

        messageText.text = DistortMessage(messageStrings[messageIndex]);
    }

    private string DistortMessage(string inString)
    {
        char[] charArray = inString.ToCharArray();
        int amountToReplace = 0;
        switch (distortionLevel)
        {
            case 0:
                return inString;
            case 1:
                amountToReplace = Mathf.FloorToInt(charArray.Length * .25f);
                break;
            case 2:
                amountToReplace = Mathf.FloorToInt(charArray.Length * .5f);
                break;
            case 3:
                amountToReplace = Mathf.FloorToInt(charArray.Length * .75f);
                break;
            case 4:
                amountToReplace = Mathf.FloorToInt(charArray.Length * .87f);
                break;
        }

        int[] availableIndices = new int[charArray.Length];
        for (int i = 0; i < availableIndices.Length; i++)
        {
            availableIndices[i] = i;
        }

        for (int i = 0; i < amountToReplace; i++)
        {
            // Pick a random index from our remaining pool
            int randomIndex = Random.Range(i, availableIndices.Length);

            // Get the actual string index from the pool
            int targetIndex = availableIndices[randomIndex];

            // Replace the character
            charArray[targetIndex] = '.';

            // Swap the used index to the 'front' of our available pool so it's not picked again
            int temp = availableIndices[i];
            availableIndices[i] = availableIndices[randomIndex];
            availableIndices[randomIndex] = temp;
        }

        return new string(charArray);
    }

    private IEnumerator CycleMessages()
    {
        while(true)
        {
            yield return new WaitForSeconds(cycleDuration);
            yield return StartCoroutine(FadeOutMessage());
            DisplayNextMessage();
            yield return StartCoroutine(FadeInMessage());
        }
        
    }

    private IEnumerator FadeOutMessage()
    {
        for (int i = 0; i < 20f; i++)
        {
            messageText.alpha = Mathf.Clamp01(1f - (i * .05f));
            yield return new WaitForSeconds(.0375f);
        }
        yield break;
    }

    private IEnumerator FadeInMessage()
    {
        for (int i = 0; i < 20f; i++)
        {
            messageText.alpha = Mathf.Clamp01(0f + (i * .05f));
            yield return new WaitForSeconds(.0375f);
        }
        yield break;
    }
}
