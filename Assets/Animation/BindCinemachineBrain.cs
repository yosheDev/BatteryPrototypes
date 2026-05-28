using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Collections;

public class BindCinemachineBrain : MonoBehaviour
{
    public PlayableDirector director;
    public List<CinemachineCamera> cineCameras = new List<CinemachineCamera>();
    public List<string> cineCameraBindings = new List<string>();

    public void Start()
    {
        // Guarentees "PlayerMainCam" will always refer to correct cam.
        cineCameras.Add(CameraManager.instance._currentCamera);
        cineCameraBindings.Add("PlayerMainCam");

        // Remaining execution must be delayed as Camera.main.tranform.parent is null at this moment.
        StartCoroutine(DelayStart());
    }

    public IEnumerator DelayStart()
    {
        yield return null;
        BindBrain(director, Camera.main.transform.parent.GetComponent<CinemachineBrain>());
    }

    public void BindBrain(PlayableDirector director, CinemachineBrain brain)
    {
        director.RebuildGraph();

        var graph = director.playableGraph;

        for (int i = 0; i < graph.GetOutputCount(); i++)
        {
            var output = graph.GetOutput(i);

            Debug.Log(
                $"Output {i}: " +
                output.GetPlayableOutputType()
            );
        }

        director.Stop();

        // Destroy existing graph completely
        if (director.playableGraph.IsValid())
        {
            director.playableGraph.Destroy();
        }

        TimelineAsset timeline = director.playableAsset as TimelineAsset;

        foreach (var track in timeline.GetOutputTracks())
        {
            // Identify the Cinemachine Track
            if (track.name == "Cinemachine Track")//(track is CinemachineTrack)
            {
                director.SetGenericBinding(track, brain);

                Debug.Log("Should have set binding. | " + track + " and " + brain);

                foreach (var clip in track.GetClips())
                {
                    for (int i = 0; i < cineCameraBindings.Count; i++)
                    {
                        // Match the clip by name
                        if (clip.displayName == cineCameraBindings[i])
                        {
                            CinemachineShot shot = clip.asset as CinemachineShot;

                            if (shot != null)
                            {
                                director.SetReferenceValue(shot.VirtualCamera.exposedName, cineCameras[i]);
                                Debug.Log("Should have set cinemachine camera binding. " + shot.VirtualCamera.exposedName + " | " + cineCameras[i]);
                            }                 
                        }
                    }
                }

                director.RebuildGraph();
                director.Evaluate();
                director.Play();

                break;
            }
        }
    }
}
