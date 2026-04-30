using Unity.VisualScripting;
using UnityEngine;

public class AudioControl : MonoBehaviour
{
    private enum AudioControlMode
    {
        FadeOut,
        PlayMusic,
        FadeIn
    }
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool triggered = false;

    [Header("Control")]
    [SerializeField] private AudioControlMode controlMode;
    [SerializeField] private bool forceRestartClip = false;

    [Header("Play Music")]
    [SerializeField] private AudioClip musicToPlay;
    [SerializeField] private float musicVolume = .08f;

    void Start()
    {
        spriteRenderer.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered)
        {
            return;
        }

        if (collision.gameObject.GetComponent<BatteryController>() != null)
        {
            switch(controlMode)
            {
                case AudioControlMode.FadeOut:
                    AudioManager.instance.FadeOutMusic(2f);
                    break;
                case AudioControlMode.PlayMusic:
                    AudioManager.instance.PlayMusicClip(musicToPlay, musicVolume, forceRestartClip);
                    break;
                case AudioControlMode.FadeIn:
                    AudioManager.instance.FadeInMusic(2f);
                    break;
                default:
                    break;
            }

            triggered = true;
        }
    }
}
