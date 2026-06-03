using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;
using Magnet;

public class a1_r7_CamBoundsUpdater : MonoBehaviour, IInterfaceEvent
{
    public GameObject hatchRoomBounds;

    public List<GameObject> bottomBounds;

    public void Start()
    {
        hatchRoomBounds.SetActive(true);

        foreach (GameObject obj in bottomBounds)
        {
            obj.SetActive(false);
        }

        CameraManager.instance.UpdateConfinedBounds();
    }
    public void InterfaceEvent(string eventName)
    {
        switch(eventName)
        {
            case "HatchOpened":
                foreach (GameObject obj in bottomBounds)
                {
                    obj.SetActive(true);
                }

                hatchRoomBounds.SetActive(false);

                CameraManager.instance.UpdateConfinedBounds();
                break;
                
            default:
                break;
        }
    }
}
