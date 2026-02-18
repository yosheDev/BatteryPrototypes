using FunctionLibrary;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines.ExtrusionShapes;

public class MagneticDistanceFieldsManager : MonoBehaviour
{
    [SerializeField] private ComputeShader sdfCompute;
    ComputeBuffer circleBuffer;
    ComputeBuffer boxBuffer;
    ComputeBuffer capsuleBuffer;
    ComputeBuffer polyBuffer;

    private RenderTexture sdfTex;

    private HashSet<Collider2D> magFieldCols = new HashSet<Collider2D>();       /// HashSet containing all magnetic field colliders in the current room.
    private List<SDFTexBounds> sdfTexBounds = new List<SDFTexBounds>();         /// HashSet containing all sdfTextureBounds in current room.
    private List<float> sdfTexPixelWSWidths = new List<float>();                /// Keeps track of a pixels width in world space. Since resolution is square, this applies for height too.
    private List<Vector2> pixelWSPositions = new List<Vector2>();               /// Caches pixelWSPositions so it does not need to be recalculated every time.

    // used in compute
    private Vector3 minBounds;
    private Vector3 maxBounds;

    private Color[] colors;

    #region Collider Data Structures
    struct CircleData
    {
        public Vector2 center;
        public float radius;
    }
    struct BoxData
    {
        public Vector2 center;
        public Vector2 halfExtents;
        public float rotation; // In radians
    }
    struct CapsuleData
    {
        public Vector2 pointA;
        public Vector2 pointB;
        public float radius;
    }
    #endregion

    [Header("Settings")]
    [SerializeField] private uint resolution = 512;

    #region List Utility
    public void ClearFieldColliders()
    {
        magFieldCols.Clear();       /// Sets container size to 0, dereferences Colliders.
        magFieldCols.TrimExcess();  /// Resize the internal storage to the minimum required capacity.
    }
    public void AddFieldCollider(Collider2D colToAdd)
    {
        magFieldCols.Add(colToAdd);
    }

    public void AddTexBounds(SDFTexBounds texBounds)
    {
        sdfTexBounds.Add(texBounds);
        float aabbWidth = texBounds.boundsWS.size.x;    /// Gets width. Assuming these are all square at the moment, so do not even worry about height.
        float pixelWSWidth = aabbWidth / resolution;

        // NOTE: If doing the multiple bounds concept, will need to make these into a texture array or for loops in shaders.
        minBounds = texBounds.boundsWS.min;
        maxBounds = texBounds.boundsWS.max;

        Shader.SetGlobalVector("_MagnetSDFMinBounds", texBounds.boundsWS.min);
        Shader.SetGlobalVector("_MagnetSDFMaxBounds", texBounds.boundsWS.max);
        sdfTexPixelWSWidths.Add(pixelWSWidth);
    }
    #endregion

    private void Awake()
    {
        // Instantiate Texture
        sdfTex = new RenderTexture((int)resolution, (int)resolution, 0);
        sdfTex.format = RenderTextureFormat.R16;
        sdfTex.enableRandomWrite = true;        /// Allows compute shader to write to individual pixels.
        sdfTex.Create();
    }

    // === NOTE ===
    /// Fields add themselves to magFieldCols on Awake(). So by the time for Start(), the HashSet should be instantiated. This also applies to sdfTextureBounds.
    void Start()
    {
        // Instantiate buffers with minimal count.
        circleBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        boxBuffer = new ComputeBuffer(1, sizeof(float) * 5);
        capsuleBuffer = new ComputeBuffer(1, sizeof(float) * 5);
        polyBuffer = new ComputeBuffer(1, sizeof(float) * 2);
    }

    void Update()
    {
        UpdateTextureWithCompute();
    }

    #region Update SDF Texture
    public void UpdateTextureWithCompute()
    {
        #region Populate Geometry Structures
        List<CircleData> circles = new List<CircleData>();
        List<BoxData> boxes = new List<BoxData>();
        List<CapsuleData> capsules = new List<CapsuleData>();
        List<Vector2> polySegments = new List<Vector2>();

        foreach (Collider2D col in magFieldCols)
        {
            // TO DO: Double check these and ensure they work with hierarchies.
            if (col is CircleCollider2D circle)
            {
                CircleData data = new CircleData();
                Transform t = circle.transform;

                data.center = (Vector2)t.TransformPoint(col.offset); /// World center that accounts for any offset.

                // Make sure scale is affecting radius.
                float maxScale = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.y));

                data.radius = circle.radius * maxScale;
                circles.Add(data);
            }
            else if (col is BoxCollider2D box)
            {
                BoxData data = new BoxData();
                Transform t = box.transform;

                data.center = (Vector2)t.TransformPoint(box.offset);

                // Make sure size is scaled correctly.
                Vector2 scaledSize = Vector2.Scale(box.size, t.lossyScale);

                data.halfExtents = scaledSize * 0.5f;

                // This is probably wrong, since the eulerAngles will exist on the parent and not on this obj itself. Double check that before fixing.
                data.rotation = t.eulerAngles.z * Mathf.Deg2Rad;

                boxes.Add(data);
            }
            else if (col is CapsuleCollider2D capsule)
            {
                CapsuleData data = new CapsuleData();
                Transform t = capsule.transform;

                Vector2 worldCenter = (Vector2)t.TransformPoint(capsule.offset);

                // Make sure scale is correctly accounted for.
                Vector2 scaledSize = Vector2.Scale(capsule.size, t.lossyScale);

                float radius;
                Vector2 localA, localB;

                if (capsule.direction == CapsuleDirection2D.Vertical)
                {
                    radius = scaledSize.x * 0.5f;

                    float height = scaledSize.y - 2f * radius;

                    localA = Vector2.up * (height * 0.5f);
                    localB = Vector2.down * (height * 0.5f);
                }
                else
                {
                    radius = scaledSize.y * 0.5f;

                    float width = scaledSize.x - 2f * radius;

                    localA = Vector2.right * (width * 0.5f);
                    localB = Vector2.left * (width * 0.5f);
                }

                // Apply rotation
                float angle = t.eulerAngles.z * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                Vector2 Rotate(Vector2 v)
                {
                    return new Vector2(
                        cos * v.x - sin * v.y,
                        sin * v.x + cos * v.y
                    );
                }

                Vector2 worldA = worldCenter + Rotate(localA);
                Vector2 worldB = worldCenter + Rotate(localB);

                data.pointA = worldA;
                data.pointB = worldB;
                data.radius = radius;

                capsules.Add(data);
            }
            else if (col is PolygonCollider2D poly)
            {
                for (int p = 0; p < poly.pathCount; p++)
                {
                    var path = poly.GetPath(p);

                    for (int i = 0; i < path.Length; i++)
                    {
                        Vector2 a = poly.transform.TransformPoint(path[i]);
                        Vector2 b = poly.transform.TransformPoint(
                            path[(i + 1) % path.Length]);

                        polySegments.Add(a);
                        polySegments.Add(b);
                    }
                }
            }
            else
            {
                Debug.LogWarning("There is no functionality added for this Collider2D type in MagneticDistanceFieldManager. Compatable types currently are: Circle, Box, Capsule, and Poly.");
            }
        }
        #endregion

        #region Setup Buffers

        if (circleBuffer.count < circles.Count)
        {
            circleBuffer.Release();
            circleBuffer = new ComputeBuffer(circles.Count, sizeof(float) * 3);
        }
        circleBuffer.SetData(circles);

        if (boxBuffer.count < boxes.Count)
        {
            boxBuffer.Release();
            boxBuffer = new ComputeBuffer(boxes.Count, sizeof(float) * 5);
        }
        boxBuffer.SetData(boxes);

        if (capsuleBuffer.count < capsules.Count)
        {
            capsuleBuffer.Release();
            capsuleBuffer = new ComputeBuffer(capsules.Count, sizeof(float) * 5);
        }
        capsuleBuffer.SetData(capsules);

        if (polyBuffer.count < polySegments.Count)
        {
           polyBuffer.Release();
           polyBuffer = new ComputeBuffer(polySegments.Count, sizeof(float) * 2);
        }
        polyBuffer.SetData(polySegments);

        #endregion

        #region Dispatch Compute

        int kernel = 0;//sdfCompute.FindKernel("CSMain");

        sdfCompute.SetTexture(kernel, "texResult", sdfTex);
        sdfCompute.SetBuffer(kernel, "circlesBuf", circleBuffer);
        sdfCompute.SetBuffer(kernel, "boxesBuf", boxBuffer);
        sdfCompute.SetBuffer(kernel, "capsulesBuf", capsuleBuffer);
        sdfCompute.SetBuffer(kernel, "polySegmentsBuf", polyBuffer);

        sdfCompute.SetInt("circlesCount", circles.Count);
        sdfCompute.SetInt("boxesCount", boxes.Count);
        sdfCompute.SetInt("capsulesCount", capsules.Count);
        sdfCompute.SetInt("polyCount", polySegments.Count / 2);

        sdfCompute.SetInt("resolution", (int)resolution);
        sdfCompute.SetVector("minBounds", minBounds);
        sdfCompute.SetVector("maxBounds", maxBounds);

        int threadGroups = Mathf.CeilToInt(resolution / 8f);        // Float should be same num as threads on compute shader
        sdfCompute.Dispatch(kernel, threadGroups, threadGroups, 1);
        #endregion

        Shader.SetGlobalTexture("_MagnetSDF", sdfTex);
    }

    #endregion

    #region Old C# Implementation
    //private float GetShortestDistanceToMagSurface(Vector2 posWS)
    //{
    //    float shortestDistance = 99999f;
    //    foreach (Collider2D col in magFieldCols)
    //    {
    //        float curDistance = Vector2.Distance(posWS, col.ClosestPoint(posWS));
    //        if (curDistance < shortestDistance)
    //        {
    //            shortestDistance = curDistance;
    //        }
    //    }
    //    return shortestDistance;
    //}

    //private void CachePixelWS()
    //{
    //    // Since pixelWS never changes in runtime, it can be generated once and reused in UpdateTexture to cache it.
    //    for (int y = 0; y < resolution; y++)
    //    {
    //        for (int x = 0; x < resolution; x++)
    //        {
    //            pixelWSPositions.Add(new Vector2(sdfTexBounds[0].boundsWS.min.x + (x * sdfTexPixelWSWidths[0]), sdfTexBounds[0].boundsWS.min.y + (y * sdfTexPixelWSWidths[0])));
    //        }
    //    }
    //}

    //private void UpdateTexture()
    //{
    //    // Will need to see into ways about making this faster probably. ECS with multithreading is the main thing looking at.
    //    for (int y = 0; y < resolution; y++)
    //    {
    //        for (int x = 0; x < resolution; x++)
    //        {
    //            // Get shortest distance from pixelWS to any of the colliders here.
    //            float distance = GetShortestDistanceToMagSurface(pixelWSPositions[(int)(x + (y * resolution))]);
    //            distance = FunctionLibraryF.MapRangeClamped(0f, 5f, 0f, 1f, distance); // 65535f

    //            // Apply to colors array (Texture format is 16-bit so 65,535 per channel)
    //            colors[x + (y * resolution)] = new Color(distance, 0f, 0f, 1f);
    //        }
    //    }

    //    //sdfTex.SetPixels(colors);
    //    //sdfTex.Apply();
    //    Shader.SetGlobalTexture("_MagnetSDF", sdfTex);
    //}
    #endregion
}
