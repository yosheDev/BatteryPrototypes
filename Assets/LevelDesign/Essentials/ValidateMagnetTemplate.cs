using UnityEditor;
using UnityEngine;

public class ValidateMagnetTemplate : MonoBehaviour
{
    [SerializeField] private Transform surface;

    private void Awake()
    {
        Validate();
    }

    public void Validate()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        surface.localPosition = Vector3.zero;
        surface.localRotation = Quaternion.identity;
    }
    private void OnValidate()
    {
        Validate();
    }
}

#region Inspector Buttons
[CustomEditor(typeof(ValidateMagnetTemplate))]
class EnsureNormalizedScaleGUI : Editor
{
    private ValidateMagnetTemplate _ensureNormalizedScale;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Validate"))
        {
            _ensureNormalizedScale = (ValidateMagnetTemplate)target;
            _ensureNormalizedScale.Validate();
        }
    }
}
#endregion
