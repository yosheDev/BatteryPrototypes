using Magnet;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mechanism : MonoBehaviour, IInterfaceEvent
{
    public bool isPowered = false;

    public List<GameObject> machineParts = new List<GameObject>();
    public List<string> machineOnEvents = new List<string>();           /// Events to call on machineParts when machine is powered on.
    public List <string> machineOffEvents = new List<string>();         /// Events to call on machienParts when machine is powered off.

    // Have child classes extend for any other functionalities and data.
    public void InterfaceEvent(string eventName)
    {
        switch (eventName)
        {
            case "PowerOn":
                for (int i = 0; i < machineParts.Count; i++)
                {
                    try
                    {
                        machineParts[i].GetComponent<IInterfaceEvent>().InterfaceEvent(machineOnEvents[i]);
                    }
                    catch
                    {
                        try
                        {
                            Debug.LogError(machineParts[i] + " does not implement the IInterfaceEvent interface.");
                        }
                        catch
                        {
                            Debug.LogError("machineParts index of " + i + " does not exist.");
                        }
                    }
                }
                break;

            case "PowerOff":
                for (int i = 0; i < machineParts.Count; i++)
                {
                    try
                    {
                        machineParts[i].GetComponent<IInterfaceEvent>().InterfaceEvent(machineOffEvents[i]);
                    }
                    catch
                    {
                        try
                        {
                            Debug.LogError(machineParts[i] + " does not implement the IInterfaceEvent interface.");
                        }
                        catch
                        {
                            Debug.LogError("machineParts index of " + i + " does not exist.");
                        }
                    }
                }
                break;
        }
    }
}
