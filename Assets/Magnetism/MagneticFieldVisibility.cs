using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MagneticFieldVisibility : MonoBehaviour
{
    [SerializeField] private Collider2D fieldCol;
    private Bounds fieldLocalBounds;

    [SerializeField] private SpriteRenderer vfxSprite;
    private Bounds vfxLocalBounds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CalculateVFXBounds();
    }

    private void CalculateVFXBounds()
    {
        // This is NOT accounting for rotations! I think GetShapeBounds / fieldLocalBounds is not actually doing what I want.

        // Get AABB that accounts for orientation.
        List<Bounds> fieldBoundsList = new List<Bounds>();
        fieldLocalBounds = fieldCol.GetShapeBounds(fieldBoundsList, true, false);

        // Get AABB that accounts for orientation.
        vfxLocalBounds = vfxSprite.localBounds;
        Vector3 curVFXBoundsScale = vfxLocalBounds.size;

        // Calculate the required scale factor for each axis
        float xScale = fieldLocalBounds.size.x / curVFXBoundsScale.x;
        float yScale = fieldLocalBounds.size.y / curVFXBoundsScale.y;
        float zScale = 1f;

        // Apply the new local scale
        transform.localScale = new Vector3(xScale / transform.parent.lossyScale.x, yScale / transform.parent.lossyScale.y, zScale);
    }

    #region Inspector Buttons
    [CustomEditor(typeof(MagneticFieldVisibility))]
    class MagneticFieldVisibilityGUI : Editor
    {
        private MagneticFieldVisibility _fieldVisibility;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Orient Field VFX"))
            {
                _fieldVisibility = (MagneticFieldVisibility)target;
                _fieldVisibility.CalculateVFXBounds();
            }
        }
    }
    #endregion
}
