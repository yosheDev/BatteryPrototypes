using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChargeParticleManager : MonoBehaviour
{
    // I need to make it so it is an in-editor tool that records the positions.
    // Add button that goes through list and populates location list.
    // Also make a button that does the opposite, and creates gameobjects at the locations from the locations list (for moving stuff around later.)

    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private GameObject _dummyObjPrefab;
    [Tooltip("Locations list is populated with the locations of these dummy actors.")]
    public List<GameObject> particleLocationDummies = new List<GameObject>();
    [SerializeField][HideInInspector] private List<Vector3> particleLocations = new List<Vector3>();
    [ReadOnly] public int locationsCount;

    // Charge Particle Settings
    [Header("Particle Global Settings")]
    public Vector2 chargeParGlobalSizeRange = new Vector2(.2f, .3f);


    private void Start()
    {
        locationsCount = particleLocations.Count;
        // Emit particles at all desired locations
        if (particleLocations.Count > 0)
        {
            for (int i = 0; i < particleLocations.Count; i++)
            {
                // Set start parameters for particle system module.
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.startColor = Color.green;
                emitParams.startSize = Random.Range(chargeParGlobalSizeRange.x, chargeParGlobalSizeRange.y);
                emitParams.startLifetime = 999999f;
                emitParams.position = particleLocations[i];

                // Spawn particle via burst
                _particleSystem.Emit(emitParams, 1);
            }
        }
    }

    public void EmitChargeParticle(ParticleSystem.EmitParams emitParams)
    {
        _particleSystem.Emit(emitParams, 1);
    }

    #region Editor Button List Management
    public void PopulatePositionsList()
    {
        Debug.Log("populate positions");
        particleLocations.Clear();

        for (int i = 0; i < particleLocationDummies.Count; i++)
        {
            particleLocations.Add(particleLocationDummies[i].transform.position);
        }

        particleLocations.TrimExcess();
        locationsCount = particleLocations.Count;
        EditorUtility.SetDirty(this);
    }
    public void CreateDummyObjects()
    {
        Debug.Log("create dummy objs");
        for (int i = 0; i < particleLocationDummies.Count; i++)
        {
            if (particleLocationDummies[i] != null)
            {
                Destroy(particleLocationDummies[i]);
            }
        }

        particleLocationDummies.Clear();

        for (int i = 0; i < particleLocations.Count; i++)
        {
            GameObject newObj = Instantiate(_dummyObjPrefab, particleLocations[i], Quaternion.identity);
            particleLocationDummies.Add(newObj);
        }

        particleLocationDummies.TrimExcess();
        EditorUtility.SetDirty(this);
    }

    public void RefreshInspectorCount()
    {
        locationsCount = particleLocations.Count;
    }
    #endregion

    #region Inspector Buttons
    [CustomEditor(typeof(ChargeParticleManager))]
    class ChargeParticleManagerGUI : Editor
    {
        private ChargeParticleManager _particleManager;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Populate Positions List from Dummy Objects"))
            {
                _particleManager = (ChargeParticleManager)target;
                _particleManager.PopulatePositionsList();
            }

            if (GUILayout.Button("Instantiate Dummy Objects from Positions List"))
            {
                _particleManager = (ChargeParticleManager)target;
                _particleManager.CreateDummyObjects();
            }

            if (GUILayout.Button("Refesh Location Count Display"))
            {
                _particleManager = (ChargeParticleManager)target;
                _particleManager.RefreshInspectorCount();
            }
        }
    }
    #endregion

    #region Custom ReadOnly Attribute
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endregion
}
