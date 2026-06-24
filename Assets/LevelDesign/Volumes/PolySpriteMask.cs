using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(SpriteMask))]
public class PolySpriteMask : MonoBehaviour
{
    private PolygonCollider2D polyCollider;
    private SpriteMask spriteMask;

    void Awake()
    {
        polyCollider = GetComponent<PolygonCollider2D>();
        spriteMask = GetComponent<SpriteMask>();
        UpdateMaskSprite();
    }

    public void UpdateMaskSprite()
    {
        // Generate a new texture based on the polygon shape
        int width = 512;
        int height = 512;
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        // Fill the texture with transparent by default
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        // Simple point-in-polygon algorithm to draw white inside the polygon
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Normalize to the collider's coordinate space (-1 to 1)
                Vector2 worldPoint = new Vector2(
                    (x / (float)width) * 2 - 1,
                    (y / (float)height) * 2 - 1
                );

                if (IsPointInPolygon(polyCollider.points, worldPoint))
                {
                    pixels[y * width + x] = Color.white;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Create a sprite from the texture and assign it to the mask
        Sprite maskSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        spriteMask.sprite = maskSprite;
    }

    private bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
    {
        bool isInside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }
}
