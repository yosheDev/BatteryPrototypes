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
    void Start()
    {
        for (int i = 0; i < areas.Count; i++)
        {
            //Scene buttonScene = SceneManager.GetSceneByBuildIndex(areas[i]);
            string scenePath = SceneUtility.GetScenePathByBuildIndex(areas[i]);
            string sceneName = scenePath;

            GameObject newButton = Instantiate(areaButtonPrefab);
            newButton.GetComponent<AreaSelectButton>().Initialize(scenePath, sceneName); // TO DO: Search database for correct name from the buttonScene area it is intending to load.
            newButton.transform.SetParent(buttonParent.transform);
            newButton.transform.localScale = Vector3.one;
        }    
    }
}
