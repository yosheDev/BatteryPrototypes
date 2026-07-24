using FunctionLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

        // Set mesh shader distance value to be the distance 
        if (surfaceCol != null)
        {
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_EmitDistance", surfaceCol.gameObject.GetComponent<MagneticSurface>()._fieldAttractDistance);
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_EmitSpeed", FunctionLibraryF.MapRangeClamped(1, 6, -13, -40, surfaceCol.gameObject.GetComponent<MagneticSurface>()._magData.strength));
            gameObject.GetComponent<MeshRenderer>().material.SetFloat("_OpacityAttenuation", Mathf.Clamp(surfaceCol.gameObject.GetComponent<MagneticSurface>()._magData.attenuation - 0.25f, 0f, 99f) * surfaceCol.gameObject.GetComponent<MagneticSurface>()._attenuationModifier);
        }
        else
        {
            Debug.LogError("surfaceCol is null on " + this.gameObject + ". surfaceCol is needed for setting variables in the mag field shader.");
        }
    }

    private void CreateShapeMesh()
    {
        shapeMesh = fieldCol.CreateMesh(true, true);

        if (!(fieldCol is EdgeCollider2D))
        {
            // Update mesh vertices to account for localScale.
            Vector3 offset = new Vector3(-1f * (magTransform.position.x / magTransform.lossyScale.x), -1f * (magTransform.position.y / magTransform.lossyScale.y), 0f);
            SetMeshLocalScale(new Vector3(transform.localScale.x / magTransform.lossyScale.x, transform.localScale.y / magTransform.lossyScale.y, 1f), offset);

            // Orientation here.
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, Mathf.Abs(180f - Mathf.Abs(magTransform.rotation.z)));
        }
        else
        {
            Vector3 offset = new Vector3(-1f * magTransform.position.x, -1f * magTransform.position.y, 0f);
            SetMeshLocalScale(Vector3.one, offset);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

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
        // NOTE: May need to redo this so it goes from surfaceCol outwards instead of the other way around. Right now it doesn't quite make sense to do it outside-in.
        /// Will need to optimize this after confirming if it works.
        
        List<Vector3> verts = new List<Vector3>();
        mesh.GetVertices(verts);    /// In object space.

        for( int v = 0; v < verts.Count; v++)
        {
            // If point is already inside of an occluder, keep vertex in that same location.
            Collider2D[] overlapCols = Physics2D.OverlapPointAll(transform.TransformPoint(verts[v]), occluderMask);
            bool inOccluder = false;
            for (int i = 0; i < overlapCols.Length; i++)
            {
                if (overlapCols[i].TryGetComponent<FieldOccluder>(out FieldOccluder hitOccluder))
                {
                    //Debug.DrawLine(transform.TransformPoint(verts[v]), (transform.TransformPoint(verts[v]) + new Vector3(0f, .2f, 0f)), Color.blue, 30f);
                    //Debug.DrawLine(transform.TransformPoint(verts[v]), (transform.TransformPoint(verts[v]) + new Vector3(.2f, 0f, 0f)), Color.blue, 30f);
                    inOccluder = true;
                    break;
                }
            }
            if (inOccluder)
            {
                continue;
            }

            // If point is not inside an occluder, trace back to the surfaceCol. If trace hits occluder, set vertex to be the hit location.
            RaycastHit2D[] occlusionCheck = Physics2D.LinecastAll(surfaceCol.ClosestPoint(transform.TransformPoint(verts[v])), transform.TransformPoint(verts[v]), occluderMask);
            //Debug.DrawLine(surfaceCol.ClosestPoint(transform.TransformPoint(verts[v])), transform.TransformPoint(verts[v]), Color.green, 30f);
            for (int h = 0; h < occlusionCheck.Length; h++)
            {
                // If there is a hit, set vert position and exit the loop.
                if (occlusionCheck[h].collider.TryGetComponent<FieldOccluder>(out FieldOccluder hitOccluder))
                {
                    //Debug.DrawLine(occlusionCheck[h].point, (occlusionCheck[h].point + new Vector2(0f, .2f)), Color.red, 30f);
                    //Debug.DrawLine(occlusionCheck[h].point, (occlusionCheck[h].point + new Vector2(.2f, 0f)), Color.red, 30f);
                    //Use occlusion veriable to Lerp position(some occluders may want to affect still but only slightly through walls. This can visually indicate that.) Might change later.
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

    #if UNITY_EDITOR
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
    #endif
}
