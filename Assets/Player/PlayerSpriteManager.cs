using UnityEngine;

public class PlayerSpriteManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer faceSpriteRenderer;
    public Sprite bodyMagnets;
    public Sprite bodyNoMagnets;

    public void UpdateSpriteByProgression(byte progression)
    {
        switch (progression)
        {
            case 0:
                bodySpriteRenderer.sprite = bodyNoMagnets;
                break;
            case 1:
                bodySpriteRenderer.sprite = bodyMagnets;
                break;
            default:
                break;
        }
    }
}
