using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    private CinemachineCamera[] _cameras;
    private CinemachineCamera _currentCamera;

    private CinemachinePositionComposer _positionComposer;
    private CinemachineConfiner2D _confiner;
    [SerializeField] private Collider2D _camBoundary;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
            DontDestroyOnLoad(instance);

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

    public void SetCameraDistance(float camDistance)
    {
        _positionComposer.CameraDistance = camDistance;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If this is a room, update confined camera bounds with those found in the room's scene.
        if (SceneManagement.IsSceneARoom(scene))
        {
            UpdateConfinedBounds();
        }
        else
        {
            Destroy(this);
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
