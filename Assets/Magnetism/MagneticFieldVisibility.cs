using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class MagneticFieldVisibility : MonoBehaviour
{
    #region References
    // Transforms
    [SerializeField] private Transform magTransform;

    // Magnetic Field
    [SerializeField] private Collider2D fieldCol;

    // VFX
    [SerializeField] private Mesh shapeMesh;
    [SerializeField] private MeshFilter meshFilter;
    //========================================================
    #endregion
     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnDestroy()
    {
        Destroy(shapeMesh);
    }
    
    void Start()
    {
        CreateShapeMesh();
    }

    private void CreateShapeMesh()
    {
        shapeMesh = fieldCol.CreateMesh(true, true);  
        
        // Update mesh vertices to account for localScale.
        SetMeshLocalScale(new Vector3(transform.localScale.x / magTransform.lossyScale.x, transform.localScale.y / magTransform.lossyScale.y, 1f));

        // Update mesh vertices to account for localPosition.
        Vector3 offset = new Vector3(-1f * (magTransform.position.x / magTransform.lossyScale.x), -1f * (magTransform.position.y / magTransform.lossyScale.y), 0f);
        SetMeshPivot(offset);

        // Orientation here.
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Abs(180f - Mathf.Abs(magTransform.rotation.z)));
        // Apply shape mesh to the meshFilter mesh.
        meshFilter.mesh = shapeMesh;
    }

    // IF THESE WORK, COMBINE INTO FUNCTION SO ONLY 1 FOR LOOP RUNS INSTEAD OF 2 SEPERATE ONES?
    private void SetMeshPivot(Vector3 pivotPosLS)
    {
        // Retrieve generated vertices
        Vector3[] newTris = shapeMesh.vertices;

        // Apply scaling to each vertex individually
        for (int i = 0; i < newTris.Length; i++)
        {
            // Add offset
            newTris[i].x += pivotPosLS.x;
            newTris[i].y += pivotPosLS.y;
            newTris[i].z += pivotPosLS.z;
        }

        // Assign the modified vertices back to the mesh
        shapeMesh.vertices = newTris;

        // Recalculate essential mesh properties
        shapeMesh.RecalculateNormals();
        shapeMesh.RecalculateBounds();
    }

    private void SetMeshLocalScale(Vector3 localScale)
    {
        // Retrieve generated vertices
        Vector3[] newTris = shapeMesh.vertices;

        // Apply scaling to each vertex individually
        for (int i = 0; i < newTris.Length; i++)
        {
            // Multiplying vector components applies the local scale
            newTris[i].x *= localScale.x;
            newTris[i].y *= localScale.y;
            newTris[i].z *= localScale.z;
        }

        // Assign the modified vertices back to the mesh
        shapeMesh.vertices = newTris;

        // Recalculate essential mesh properties
        shapeMesh.RecalculateNormals();
        shapeMesh.RecalculateBounds();
    }

    #region Inspector Buttons
    [CustomEditor(typeof(MagneticFieldVisibility))]
    class MagneticFieldVisibilityGUI : Editor
    {
        private MagneticFieldVisibility _fieldVisibility;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Create Shape Mesh"))
            {
                _fieldVisibility = (MagneticFieldVisibility)target;
                _fieldVisibility.CreateShapeMesh();
            }
        }
    }
    #endregion
}
