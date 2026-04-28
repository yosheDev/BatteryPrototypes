using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameInstance;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;

    private List<string> speakers = new List<string>();
    private List<string> lines = new List<string>();

    private bool inDialogue = false;

    private int dialogueIndex = 0;

    #region Delegates
    public delegate void OnDialogueStarted();
    public event OnDialogueEnded onDialogueStarted;

    public delegate void OnDialogueAdvanced();
    public event OnDialogueAdvanced onDialogueAdvanced;

    public delegate void OnDialogueEnded();
    public event OnDialogueEnded onDialogueEnded;
    #endregion

    #region Singleton
    public static DialogueManager instance;

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

    public void EndDialogue()
    {
        inDialogue = false;
        dialogueText.gameObject.SetActive(false);
        onDialogueEnded?.Invoke();
    }

    public void BeginDialogue(string csvName)
    {
        bool dataLoaded = LoadData(csvName);
        if (!dataLoaded)
        {
            return;
        }

        inDialogue = true;
        dialogueText.gameObject.SetActive(true);
        onDialogueStarted?.Invoke();

        dialogueIndex = 0;
        DisplayLine(dialogueIndex);
    }

    public void AdvanceDialogue()
    {
        dialogueIndex++;

        // Is dialogue over?
        if (dialogueIndex >= lines.Count)
        {
            EndDialogue();
            return;
        }

        DisplayLine(dialogueIndex);
        onDialogueAdvanced?.Invoke();
    }
    public void DisplayLine(int displayIndex)
    {
        if (displayIndex >= lines.Count)
        {
            Debug.LogError("Cannot display dialogue. DisplayLine(displayIndex) is greater than container size.");
            return;
        }
        dialogueText.SetText(lines[displayIndex]);
    }

    private bool LoadData(string csvName)
    {
        TextAsset csv = Resources.Load<TextAsset>("Dialogue" + csvName);

        if (csv == null)
        {
            Debug.LogError("Failed to load dialogue data. Resources/Dialogue/" + csvName + ".csv was not found.");
            return false;
        }

        #region Extract Data and Populate Lists
        // Split the csv into lines(rows).
        string[] rows = csv.text.Split('\n');

        // Loop through lines(rows) to seperate columns based on commas. (starting at 1 to skip header)
        for (int i = 1; i < rows.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // Split line into columns
            string[] columns = lines[i].Split(',');

            // Assuming CSV structure: Speaker, Line(English), 
            speakers.Add(columns[0]);
            lines.Add(columns[1]);

        }
        return true;
        #endregion
    }
}
