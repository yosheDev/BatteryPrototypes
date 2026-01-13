using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Debug.Log("Camera Manager is Awake");
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
                UpdateConfinedBounds();
            }
        }

        // TO DO: Set collider on the bounds extension.
    }

    private void Start()
    {
        Debug.Log("What did I find: " + GameObject.FindWithTag("CameraBoundary"));
    }
    public void UpdateConfinedBounds()
    {
        Debug.Log("Updating confined bounds in Camera Manager.");
        if (_confiner == null)
        {
            Debug.LogError("ERROR: Tried to update bounds for CinemachineConfiner2D, but Camera Manager _confiner is null.");
            return;
        }
        
        if (GameObject.FindWithTag("CameraBoundary").GetComponent<Collider2D>() == null)
        {
            Debug.LogError("ERROR: Tried to update bounds for CinemachineConfiner2D, but could not find a composite collider on a GameObject with the tag CameraBoundary.");
            return;
        }

        _camBoundary = GameObject.FindWithTag("CameraBoundary").GetComponent<Collider2D>();
        _confiner.BoundingShape2D = _camBoundary;
    }
}
