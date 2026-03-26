using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private CinemachineCamera[] _cameras;
    public CinemachineCamera _currentCamera;

    private CinemachinePositionComposer _positionComposer;
    private CinemachineTargetGroup _targetGroup;
    private CinemachineConfiner2D _confiner;
    [SerializeField] private Collider2D _camBoundary;

    private Dictionary<Transform, Coroutine> removeTargetCoroutines = new Dictionary<Transform, Coroutine>();
    private Dictionary<Transform, Coroutine> addTargetCoroutines = new Dictionary<Transform, Coroutine>();

    private HashSet<CameraZoomVolume> overlappedZoomVolumes = new HashSet<CameraZoomVolume>();
    private Coroutine camZoomRoutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Destroying duplicate of " + instance + ". The duplicate is: " + this);
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(instance);

        _targetGroup = gameObject.GetComponentInChildren<CinemachineTargetGroup>();

        _cameras = this.transform.GetComponentsInChildren<CinemachineCamera>();

        for (int i = 0; i < _cameras.Length; i++)
        {
            if (_cameras[i].enabled == true)
            {
                _currentCamera = _cameras[i];
                _positionComposer = _currentCamera.gameObject.GetComponent<CinemachinePositionComposer>();

                _confiner = _currentCamera.gameObject.GetComponent<CinemachineConfiner2D>();

                // Bind event to scene loaded. This event will be responsible for handling dynamic adjustements to _confiner and _camBoundary upon loading new rooms.
                SceneManager.sceneLoaded += OnRoomLoaded;
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Camera manager was destroyed.");
        SceneManager.sceneLoaded -= OnRoomLoaded;
    }

    #region Getters
    public CinemachineCamera GetCurrentCamera()
    {
        return _currentCamera;
    }

    public CinemachinePositionComposer GetPositionComposer()
    {
        return _positionComposer;
    }

    public CinemachineConfiner2D GetConfiner()
    {
        return _confiner;
    }
    #endregion

    #region Follow Target Utility
    /// <summary>
    /// Adds a follow target to the cinemachine target group. Duration controls how quickly the weight blends in from 0 to 1. Uses fixedDeltaTime.
    /// </summary>
    public void AddFollowTarget(Transform target, float addDuration = 0.5f)
    {
        // If there is still a remove routine with this target in it, cancel it and remove from the Dictionary.
        if (removeTargetCoroutines.ContainsKey(target))
        {
            // If a coroutine is still running to remove this target, cancel it.
            removeTargetCoroutines.TryGetValue(target, out Coroutine removeRoutine);
            if (removeRoutine != null)
            {
                //Debug.Log("Upon adding new target: Stopping and removing existing remove coroutine on that target.");
                StopCoroutine(removeRoutine);
            }

            removeTargetCoroutines.Remove(target);
        }

        if (addDuration <= 0f)
        {
            if (_targetGroup.FindMember(target) == -1)
            {
                _targetGroup.AddMember(target, 1f, 0f);
            }
        }
        else
        {
            if (_targetGroup.FindMember(target) == -1)
            {
                _targetGroup.AddMember(target, 0f, 0f);
            }
            
            // Start coroutine, and add it to the Dictionary so it can be cancelled later if it needs to be.
            Coroutine addTargetCoroutine = CameraManager.instance.StartCoroutine(BlendInFollowTarget(target, addDuration));
            addTargetCoroutines.Add(target, addTargetCoroutine);
        }
    }

    /// <summary>
    /// Adds a follow target to the cinemachine target group. Duration controls how quickly the weight blends in from 0 to 1. Uses fixedDeltaTime.
    /// </summary>
    public void RemoveFollowTarget(Transform target, float removeDuration = 0.5f)
    {
        // If there is still an add routine with this target in it, cancel it and remove from both the target group and Dictionary.
        if (addTargetCoroutines.ContainsKey(target))
        {            
            addTargetCoroutines.TryGetValue(target, out Coroutine addRoutine);
            if (addRoutine != null)
            {
                //Debug.Log("Upon removing target: Stopping and removing existing add coroutine on that target.");
                StopCoroutine(addRoutine);
            }

            addTargetCoroutines.Remove(target);
        }

        if (removeDuration <= 0f)
        {
            _targetGroup.RemoveMember(target);
        }
        else
        {
            // Start coroutine, and add it to the Dictionary so it can be cancelled later if it needs to be.
            Coroutine removeTargetCoroutine = CameraManager.instance.StartCoroutine(BlendOutFollowTarget(target, removeDuration));
            removeTargetCoroutines.Add(target, removeTargetCoroutine);
        }
    }

    private IEnumerator BlendOutFollowTarget(Transform targetTrans, float duration)
    {
        CinemachineTargetGroup.Target targetToRemove = _targetGroup.Targets[_targetGroup.FindMember(targetTrans)];

        float curBlendVelocity = 0f;
        float newWeight = 0f;
        while (Mathf.Abs(1f - targetToRemove.Weight) < .98f)
        {
            newWeight = Mathf.SmoothDamp(targetToRemove.Weight, 0f, ref curBlendVelocity, duration, Mathf.Infinity, Time.fixedDeltaTime);
            targetToRemove.Weight = newWeight;
            
            //Debug.Log("Blend Out: " + targetToRemove.Weight);
            yield return new WaitForSeconds(.01f);
        }

        _targetGroup.RemoveMember(targetTrans);
        removeTargetCoroutines.Remove(targetTrans);
        //Debug.Log("I have finished removing the target.");
        yield break;
    }

    private IEnumerator BlendInFollowTarget(Transform targetTrans, float duration)
    {
        CinemachineTargetGroup.Target targetToAdd = _targetGroup.Targets[_targetGroup.FindMember(targetTrans)];

        float curBlendVelocity = 0f;
        float newWeight = 0f;
        while (Mathf.Abs(1f - targetToAdd.Weight) > .02f)
        {
            newWeight = Mathf.SmoothDamp(targetToAdd.Weight, 1f, ref curBlendVelocity, duration, Mathf.Infinity, Time.fixedDeltaTime);
            targetToAdd.Weight = newWeight;

            //Debug.Log("Blend In: " + targetToAdd.Weight);
            yield return new WaitForSeconds(.01f);
        }

        addTargetCoroutines.Remove(targetTrans);
        //Debug.Log("I have finished adding the target.");
        yield break;
    }

    #endregion

    #region Zoom Volumes Utility
    public void AddZoomVolume(CameraZoomVolume newVolume)
    {
        overlappedZoomVolumes.Add(newVolume);
    }
    public void RemoveZoomVolume(CameraZoomVolume newVolume)
    {
        overlappedZoomVolumes.Remove(newVolume);
    }

    public void BlendCameraZoomVolume(bool blendIn, float desiredZoom, float duration)
    {
        if (camZoomRoutine != null)
        {
            StopCoroutine(camZoomRoutine);
        }

        if (blendIn)
        {
            camZoomRoutine = CameraManager.instance.StartCoroutine(InterpolateCamZoomVolumes(desiredZoom, duration));
            Debug.Log("Start interp to " + desiredZoom + " additional zoom");
        }
        else
        {
            // Look for another volume or default to zero.
            if (overlappedZoomVolumes.Count <= 0)
            {
                camZoomRoutine = CameraManager.instance.StartCoroutine(InterpolateCamZoomVolumes(0f, duration));
                return;
            }
            else
            {
                float newZoom = overlappedZoomVolumes.ElementAt(0).additionalZoom;

                camZoomRoutine = CameraManager.instance.StartCoroutine(InterpolateCamZoomVolumes(newZoom, duration));
                return;
            }
        }
    }

    private IEnumerator InterpolateCamZoomVolumes(float desiredZoom, float duration = 2f)
    {
        float curBlendVelocity = 0f;
        float newZoom = GetCurrentCamera().GetComponent<DollyVelocity>().GetAdditionalZoom();

        // While interpolating within a threshold of accuracy.
        while (Mathf.Abs(GetCurrentCamera().GetComponent<DollyVelocity>().GetAdditionalZoom() - desiredZoom) >= .1f)
        {
            newZoom = Mathf.SmoothDamp(GetCurrentCamera().GetComponent<DollyVelocity>().GetAdditionalZoom(), desiredZoom, ref curBlendVelocity, duration, Mathf.Infinity, Time.fixedDeltaTime);
            GetCurrentCamera().GetComponent<DollyVelocity>().SetAdditionalZoom(newZoom);
            yield return null;
        }

        yield break;
    }
    #endregion

    #region Other Camera Utility
    public void SetCameraDistance(float camDistance)
    { 
        _positionComposer.CameraDistance = camDistance;
        //_currentCamera.Lens.OrthographicSize = camDistance; /// DISABLED cause no longer using orthographic camera.
        _confiner.InvalidateLensCache();
    }

    public float GetCameraDistance()
    {
        return _positionComposer.CameraDistance;
    }
    void OnRoomLoaded(Scene scene, LoadSceneMode mode)
    {
        // If this is a room, update confined camera bounds with those found in the room's scene.
        if (SceneManagement.IsSceneARoom(scene))
        {
            UpdateConfinedBounds();
        }
        
        /// NOTE: If cam boundaries seem to be failing, it is probably because .IsSceneARoom() is returning that it is not a room.
    }

    /// <summary>
    /// Searches for GameObject with "CameraBoundary" tag and uses the found Composite Collider as the new camera bounds.
    /// </summary>
    public void UpdateConfinedBounds()
    {
        #region Handle Exceptions
        try
        {
            if (_confiner == null) { }
        }
        catch {
            Debug.LogError("ERROR: Tried to update bounds for CinemachineConfiner2D, but Camera Manager _confiner is null.");
            return;
        }

        try { 
            if (GameObject.FindWithTag("CameraBoundary").GetComponent<Collider2D>() == null) { }
        }
        catch {
            Debug.LogError("ERROR: Tried to update bounds for CinemachineConfiner2D, but could not find a composite collider on a GameObject with the tag CameraBoundary.");
            return;
        }
        #endregion

        _camBoundary = GameObject.FindWithTag("CameraBoundary").GetComponent<Collider2D>();
        if (_camBoundary.GetComponent<Collider2D>() as CompositeCollider2D)
        {
            _camBoundary.GetComponent<CompositeCollider2D>().GenerateGeometry();
        }
        Debug.Log("Cam Boundary Set: " + _camBoundary);
        _confiner.BoundingShape2D = _camBoundary;
        _confiner.InvalidateBoundingShapeCache();
    }

    /// <summary>
    /// Warps the current camera to whatever the Follow target is.
    /// </summary>
    public void WarpCamera()
    {
        _currentCamera.PreviousStateIsValid = false;
        _currentCamera.OnTargetObjectWarped(_currentCamera.Follow, _currentCamera.Follow.position - _currentCamera.transform.position);
    }

    #endregion
}
