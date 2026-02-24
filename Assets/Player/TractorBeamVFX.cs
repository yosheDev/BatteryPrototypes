using FunctionLibrary;
using Unity.VisualScripting;
using UnityEngine;

public class TractorBeamVFX : MonoBehaviour
{
    [SerializeField] private MagnetComponentBase playerMag;     /// The player magnet associated with this VFX. Used for determing direction params of the VFX based on charge. Ref value due to possible scope of mags changing charge at runtime.
    private Vector2 playerMagPos;                               /// Pos to use for start of beam from playerMag.
    private Vector2 endPos;                                     /// Pos to use for end of beam.
    private float forceMagnitude;                               /// Force magnitude affects the width.

    private Sprite beamSprite;                          /// Sprite of the beam. May not use this.
    private SpriteRenderer spriteRenderer;
    private Material beamMaterial;                      /// Material to use for the beam.
    private float width = .5f;                          /// Width of the beam. Might be dynamic at runtime, plan as such.
    private float widthMultiplier = 1f;                 /// Global multiplier for the width. For look dev and quickly changing width.
    private Vector2 dir = Vector2.zero;                 /// Direction of the beam.

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        beamSprite = spriteRenderer.sprite;
        beamMaterial = spriteRenderer.material;   
    }

    private void FixedUpdate()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, FunctionLibraryF.MapRangeClamped(.4f, .5f, 0f, .5f, Vector2.Distance(playerMagPos, endPos)));
        SetPlayerMagPos(playerMag.transform.position);
        Vector2 centerPos = (playerMagPos + endPos) / 2f;
        transform.position = centerPos;

        dir = (playerMagPos - endPos).normalized;
        transform.right = dir;

        Vector3 scale = new Vector3(1, 1, 1);
        scale.x = Vector2.Distance(playerMagPos, endPos);
        if (forceMagnitude < 3f)
        {
            width = FunctionLibraryF.MapRangeClamped(0f, 3f, 0f, .1f, forceMagnitude) * widthMultiplier;
        }
        else if (forceMagnitude < 8f)
        {
            width = FunctionLibraryF.MapRangeClamped(3f, 8f, .1f, .4f, forceMagnitude) * widthMultiplier;
        }
        else
        {
            width = FunctionLibraryF.MapRangeClamped(8f, 14f, .4f, .7f, forceMagnitude) * widthMultiplier;
        }
        scale.y = width;
        transform.localScale = scale;
    }

    #region Utility
    private bool GetPlayerMagCharge()
    {
        return (playerMag._magData.charge == 1 ? true : false);
    }

    public void SetForceMagnitude(float newMagnitude)
    {
        forceMagnitude = newMagnitude;
    }
    public void SetPlayerMagPos(Vector2 newPos)
    {
        playerMagPos = newPos;
    }

    public void SetEndPos(Vector2 newPos)
    {
        endPos = newPos;
    }
    #endregion
}   
