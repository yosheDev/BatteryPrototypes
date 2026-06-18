using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Collections;
using Magnet;

public class BindPlayerCharacter : MonoBehaviour, IInterfaceEvent
{
    public PlayableDirector director;
    public string trackName = "PlayerAnimationTrack";

    public void InterfaceEvent(string eventName)
    {
        switch(eventName)
        {
            case "Play":
                Play();
                break;
            default:
                break;
        }
    }

    public void Play()
    {
        GameObject playerObj = GameObject.FindAnyObjectByType<BatteryController>().gameObject;

        if (playerObj == null)
        {
            Debug.LogError("playerObj is null in BindPlayerCharacter.cs on " + gameObject);
            return;
        }
        TimelineAsset timeline = director.playableAsset as TimelineAsset;

        foreach (var track in timeline.GetOutputTracks())
        {
            Debug.Log(track.name);
            if (track.name == trackName)
            {
                // Rebind the track to the real player at runtime
                director.SetGenericBinding(track, playerObj.transform.parent.gameObject);
                Debug.Log("Binding for " + track + " should now be " + playerObj);
                break;
            }
        }
        director.RebindPlayableGraphOutputs();
        director.Play();
    }
}
