using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaSelectButton : MonoBehaviour
{
    [SerializeField] private Areas area;
    [SerializeField] private TMP_Text _text;

    // Initialize calls when factory creates this.
    public void Initialize(string newScenePath, string newText)
    {
        _text.SetText(SceneManagement.GetAreaDisplayName(area));
    }

    public void EnterArea()
    {
        Level newArea = new Level(area, -1);
        SceneManagement.LoadScene(newArea);
    }
}
