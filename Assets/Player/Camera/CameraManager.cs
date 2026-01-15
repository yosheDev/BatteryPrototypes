using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private CinemachineCamera[] _cameras;
    private CinemachineCamera _currentCamera;

    private CinemachinePositionComposer _positionComposer;
    private CinemachineTargetGroup _targetGroup;
    private CinemachineConfiner2D _confiner;
    [SerializeField] private Collider2D _camBoundary;

    private List<Coroutine> removeTargetCoroutines = new List<Coroutine>();

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
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Camera manager was destroyed.");
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

    public void SetFollowTarget(Transform newTarget)
    {
        Transform previousTarget = _targetGroup.Targets[0].Object.transform;
        _targetGroup.AddMember(newTarget, 1f, 0f);
        _targetGroup.RemoveMember(previousTarget);
    }

    public void AddFollowTarget(Transform newTarget)
    {
        try
        {
            if (!(_targetGroup.Targets.Contains(_targetGroup.Targets[_targetGroup.FindMember(newTarget)])))
            {
                _targetGroup.AddMember(newTarget, 1f, 0f);
            }
        }
        catch
        {
            _targetGroup.AddMember(newTarget, 1f, 0f);
        }
    }
    
    public void RemoveFollowTarget(Transform target, float removeDuration = 0.5f)
    {
        if (removeDuration <= 0f)
        {
            _targetGroup.RemoveMember(target);
        }
        else
        {
            Coroutine removeTargetCoroutine = CameraManager.instance.StartCoroutine(BlendOutFollowTarget(target, removeDuration));
            //CameraManager.instance.removeTargetCoroutines.Add(removeTargetCoroutine);
        }
    }

    private IEnumerator BlendOutFollowTarget(Transform target, float duration)
    {
        // Currently this works linearly.

        float counter = _targetGroup.Targets[_targetGroup.FindMember(target)].Weight;
        float counterDecrement = counter / 10f;

        while (counter > 0f)
        {
            Debug.Log("In while loop");
            yield return new WaitForSeconds(duration / 10f); /// We are assuming loop runs 10 times.
            counter -= counterDecrement;
            _targetGroup.Targets[_targetGroup.FindMember(target)].Weight = counter;
        }

        _targetGroup.RemoveMember(target);
        Debug.Log("I have finished");
        yield break;
    }

    public void SetCameraDistance(float camDistance)
    {
        _positionComposer.CameraDistance = camDistance;
        _currentCamera.Lens.OrthographicSize = camDistance;
        _confiner.InvalidateLensCache(); /// Expensive but this will not work without it.
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If this is a room, update confined camera bounds with those found in the room's scene.
        if (SceneManagement.IsSceneARoom(scene))
        {
            UpdateConfinedBounds();
        }
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
        _confiner.BoundingShape2D = _camBoundary;
    }

    /// <summary>
    /// Warps the current camera to whatever the Follow target is.
    /// </summary>
    public void WarpCamera()
    {
        _currentCamera.PreviousStateIsValid = false;
        _currentCamera.OnTargetObjectWarped(_currentCamera.Follow, _currentCamera.Follow.position - _currentCamera.transform.position);
    }
}
