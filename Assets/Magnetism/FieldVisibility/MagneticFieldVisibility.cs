using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

public class MagneticFieldVisibility : MonoBehaviour
{
    // This script is responsible for generating the mesh of the Magnetic Fields at the start of play. These have to be generated dynamically as magnetic fields
    // need to match magnet shape, and this allows for quick iteration of design without losing time on assets for each specific instance.

    #region References
    // Transforms
    [SerializeField] private Transform magTransform;

    // Magnet
    [SerializeField] private Collider2D fieldCol;
    [SerializeField] private Collider2D surfaceCol;

    // VFX
    [SerializeField] private Mesh shapeMesh;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private LayerMask occluderMask;

    [SerializeField] private bool doOcclusionCheck = true;
    //========================================================
    #endregion
    
    private void OnDestroy()
    {
        Destroy(shapeMesh); /// Since meshes are manually created, they must be manually destroyed.
    }
    
    void Awake()
    {
        CreateShapeMesh();
    }

    private void CreateShapeMesh()
    {
        shapeMesh = fieldCol.CreateMesh(true, true);

        // Update mesh vertices to account for localScale.
        Vector3 offset = new Vector3(-1f * (magTransform.position.x / magTransform.lossyScale.x), -1f * (magTransform.position.y / magTransform.lossyScale.y), 0f);
        SetMeshLocalScale(new Vector3(transform.localScale.x / magTransform.lossyScale.x, transform.localScale.y / magTransform.lossyScale.y, 1f), offset);

        // Orientation here.
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Abs(180f - Mathf.Abs(magTransform.rotation.z)));

        // Alter vertice locations to hit field occluders.
        if (surfaceCol != null)
        {
            if (doOcclusionCheck)
            {
                OcclusionCheck(ref shapeMesh);
            }
        }
        else
        {
            Debug.LogError("surfaceCol is null on " + this.gameObject + ". surfaceCol is needed for field mesh occlusion check.");
        }

        // Generate UVs from planar projection.
        //GeneratePlanarUVs();

        // Apply shape mesh to the meshFilter mesh.
        meshFilter.mesh = shapeMesh;
    }

    #region Mesh Utility

    private void OcclusionCheck(ref Mesh mesh)
    {
        /// Will need to optimize this after confirming if it works.
        
        List<Vector3> verts = new List<Vector3>();
        mesh.GetVertices(verts);    /// In object space.

        for( int v = 0; v < verts.Count; v++)
        {
            RaycastHit2D[] occlusionCheck = Physics2D.LinecastAll(transform.TransformPoint(verts[v]), surfaceCol.ClosestPoint(verts[v]), occluderMask);

            for(int h = 0; h < occlusionCheck.Length; h++)
            {
                // If there is a hit, set vert position and exit the loop.
                if (occlusionCheck[h].collider.TryGetComponent<FieldOccluder>(out FieldOccluder hitOccluder))
                {
                    //Debug.DrawLine(Vector2.Lerp(transform.TransformPoint(verts[v]), occlusionCheck[h].point, hitOccluder.occlusion), Vector2.Lerp(transform.TransformPoint(verts[v]), occlusionCheck[h].point, hitOccluder.occlusion) + new Vector2(0f, .5f), Color.red, 10f);
                    // Set vertice to be where the occlusion was. Use occlusion veriable to Lerp position(some occluders may want to affect still but only slightly through walls. This can visually indicate that.)
                    verts[v] = Vector2.Lerp(verts[v], transform.InverseTransformPoint(occlusionCheck[h].point), hitOccluder.occlusion);
                    break;
                }
            }
        }

        mesh.vertices = verts.ToArray();
    }

    // These two functions were combined into one (SetMeshLocalScale) for better performance. Seems to work fine and all. Leaving here in case i do actually need it to be separated.
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

    private void SetMeshLocalScale(Vector3 localScale, Vector3 pivotPosLS)
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

    private void GeneratePlanarUVs()
    {
        Vector2[] endingUVs = new Vector2[shapeMesh.vertices.Length];
        Bounds bounds = shapeMesh.bounds;

        for (int k = 0; k < endingUVs.Length; k++)
        {
            Vector2 aUV = new Vector2((shapeMesh.vertices[k].x / bounds.size.y) * -1, (shapeMesh.vertices[k].y / bounds.size.y));
            endingUVs[k] = aUV;
        }
        shapeMesh.uv = endingUVs;
    }
    #endregion

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
