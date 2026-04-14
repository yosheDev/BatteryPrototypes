using UnityEngine;

public class AudioControl : MonoBehaviour
{
    private enum AudioControlMode
    {
        FadeOut
    }
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Control")]
    [SerializeField] private AudioControlMode controlMode;

    void Start()
    {
        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<BatteryController>() != null)
        {
            AudioManager.instance.FadeOutMusic(2f);
        }
    }
}
