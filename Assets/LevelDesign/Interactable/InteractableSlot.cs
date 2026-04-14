using Magnet;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class InteractableSlot : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private TextMeshPro displayText;

    [Header("Parameters")]
    public bool usesBattery = true;                                     /// Does this use battery?
    public float batteryCost = 25f;                                     /// Player battery cost to interact with this slot.
    public bool canOnlyUseOnce = false;                                 /// If true, slot is only interactable once.
    private bool canInteract = true;                                    /// Cannot interact with this slot when this is false.

    [Header("Interface Events")]
    public List<GameObject> eventObjects = new List<GameObject>();      /// Objects to call InterfaceEvent() on.
    public List<string> eventNames = new List<string>();                /// String passed into InterfaceEvent() to specify which is called.
    public List<float> eventEndDelays = new List<float>();              /// Delays take place after the event is called

    private Coroutine interfaceEvents;

    private void Awake()
    {
        SetDisplayState(false);
    }
    public void Interact(BatteryController playerController)
    {
        /// This function should not even be callable if canInteract is false, so don't worry about that check here.
        
        /// Does player have enough resources?
        if (playerController.battery.percent >= batteryCost || !usesBattery)
        {
            playerController.battery.SetPercent(playerController.battery.percent - batteryCost);
            SetDisplayState(false);
            if (canOnlyUseOnce)
            {
                canInteract = false;
            }
            else
            {
                // Reset can interact after shortest possible duration.
                StartCoroutine(ResetInteractable(Mathf.Max(0.2f, GetTotalInterfaceEventDelay()), playerController));
            }

            // For each event object, call the associated interface event as specified in eventNames.
            interfaceEvents = StartCoroutine(IterateInterfaceEvents());
        }
        else
        {
            Debug.Log("Not enough battery to use this!");
        }
    }

    private IEnumerator IterateInterfaceEvents()
    {
        int i = 0;
        while (i < eventObjects.Count)
        {
            try
            {
                IInterfaceEvent intEvent = eventObjects[i].GetComponent<IInterfaceEvent>();
                intEvent.InterfaceEvent(eventNames[i]);
            }
            catch { }

            // Is valid index.
            if (i >= 0 && i < eventEndDelays.Count)
            {
                if (eventEndDelays[i] > 0f)
                {
                    yield return new WaitForSeconds(eventEndDelays[i]);
                }
                else
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }

                i++;
        }
        yield break;
    }

    private IEnumerator ResetInteractable(float duration, BatteryController playerController)
    {
        yield return new WaitForSeconds(duration);
        canInteract = true;

        // Reset display if playerController still nearby this.
        if (playerController.interactObj == this)
        {
            SetDisplayState(true);
        }

        yield break;
    }

    private float GetTotalInterfaceEventDelay()
    {
        float sum = 0f;
        foreach (float delay in eventEndDelays)
        {
            sum += delay;
        }
        return sum;
    }

    public void TrySetInteractDisplay(bool display)
    {
        SetDisplayState(canInteract ? display : false);
    }

    private void SetDisplayState(bool display)
    {
        if (usesBattery)
        {
            displayText.enabled = display;
            displayText.SetText("E<br>" + batteryCost + "%");
        }
        else
        {
            displayText.enabled = display;
            displayText.SetText("E");
        }    
    }
    // NOTE: Only display interact text if can be interacted with (like not a single use interaction slot)
}
