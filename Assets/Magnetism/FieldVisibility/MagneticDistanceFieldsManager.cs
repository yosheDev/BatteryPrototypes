using FunctionLibrary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MagneticDistanceFieldsManager : MonoBehaviour
{
    private HashSet<Collider2D> magFieldCols = new HashSet<Collider2D>();       /// HashSet containing all magnetic field colliders in the current room.
    private List<SDFTexBounds> sdfTexBounds = new List<SDFTexBounds>();   /// HashSet containing all sdfTextureBounds in current room.
    private List<float> sdfTexPixelWSWidths = new List<float>();                /// Keeps track of a pixels width in world space. Since resolution is square, this applies for height too.

    private Texture2D sdfTex;
    private Color[] colors;

    [Header("Settings")]
    [SerializeField] private uint resolution = 512;
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
        Shader.SetGlobalVector("_MagnetSDFMinBounds", texBounds.boundsWS.min);
        Shader.SetGlobalVector("_MagnetSDFMaxBounds", texBounds.boundsWS.max);
        sdfTexPixelWSWidths.Add(pixelWSWidth);
    }
    private void Awake()
    {
        // Instantiate Texture
        sdfTex = new Texture2D((int)resolution, (int)resolution, TextureFormat.R16, false);
        colors = new Color[resolution * resolution];
        
    }
    // === NOTE ===
    /// Fields add themselves to magFieldCols on Awake(). So by the time for Start(), the HashSet should be instantiated. This also applies to sdfTextureBounds.
    void Start()
    {
        UpdateTexture();
    }

    void Update()
    {
        //UpdateTexture();
    }

    /// <summary>
    /// Uses Job System to get pixels nearest distance to collider from all colliders in magFieldCols.
    /// </summary>
    private void UpdateTexture()
    {
        // Will need to see into ways about making this faster probably. ECS with multithreading is the main thing looking at.

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Get pixels location in world space.
                Vector2 pixelWS = new Vector2(sdfTexBounds[0].boundsWS.min.x + (x * sdfTexPixelWSWidths[0]), sdfTexBounds[0].boundsWS.min.y + (y * sdfTexPixelWSWidths[0]));

                // Get shortest distance to any of the colliders here.
                float distance = GetShortestDistanceToMagSurface(pixelWS);
                distance = FunctionLibraryF.MapRangeClamped(0f, 5f, 0f, 1f, distance); // 65535f

                // Apply to colors array (Texture format is 16-bit so 65,535 per channel)
                colors[x + (y * resolution)] = new Color(distance, 0f, 0f, 1f);
            }
        }

        sdfTex.SetPixels(colors);
        sdfTex.Apply();
        Shader.SetGlobalTexture("_MagnetSDF", sdfTex);
    }

    private float GetShortestDistanceToMagSurface(Vector2 posWS)
    {
        float shortestDistance = 99999f;
        foreach (Collider2D col in magFieldCols)
        {
            float curDistance = Vector2.Distance(posWS, col.ClosestPoint(posWS));
            if (curDistance < shortestDistance)
            {
                shortestDistance = curDistance;
            }
        }
        return shortestDistance;
    }
}
