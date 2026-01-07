using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;

public class AreaSelection : MonoBehaviour
{
    [SerializeField] private GameObject areaButtonPrefab;
    [SerializeField] private GameObject buttonParent;
    [Tooltip("Uses build index to get correct area base scenes.")]
    [SerializeField] private List<int> areas = new List<int>();

    void Awake()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            Areas buttonArea = (Areas)i; /// This is pretty wonky and will need redone later. Basically using i to get the correct Area enum. Relies on the order of build indexes being correct in the properties panel.

            GameObject newButton = Instantiate(areaButtonPrefab);
            newButton.GetComponent<AreaSelectButton>().Initialize(buttonArea); // TO DO: Search database for correct name from the buttonScene area it is intending to load.
            newButton.transform.SetParent(buttonParent.transform);
            newButton.transform.localScale = Vector3.one;
        }    
    }
}
