using Magnet;
using System.Collections;
using System.Collections.Generic;
using Unity.ProjectAuditor.Editor;
using UnityEngine;
public class a1_r6_Events : MonoBehaviour
{
    public List<GameObject> bombingRemoveObjs = new List<GameObject>();
    public List<GameObject> returnAppearObjects = new List<GameObject>();
    public List<GameObject> ventsToOpen = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (AreaManager.instance.checkpointRespawnCount > 0)
        {
            //Debug.Log("Room is blasted open!");

            foreach(GameObject obj in bombingRemoveObjs)
            {
                Destroy(obj);
            }

            foreach (GameObject vent in ventsToOpen)
            {
                vent.GetComponent<IInterfaceEvent>().InterfaceEvent("Open");
            }

            InGameAreaTitleUI.instance.DisplayAreaTitle(Areas.Area1, 11);

        }
        else
        {
            //Debug.Log("This is the first time being in the room.");
            foreach (GameObject obj in returnAppearObjects)
            {
                obj.SetActive(false);
            }
        }
    }
}
