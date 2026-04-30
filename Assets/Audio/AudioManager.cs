using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static GameInstance;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;

    #region Singleton
    public static AudioManager instance;

    private void Awake()
    {
        // If the instance is null, this is the first and only instance
        if (instance == null)
        {
            // Set the static instance to this instance
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Music
    public void PlayMusicClip(AudioClip audioClip, float newVolume = -1f, bool forceRestartClip = false)
    {
        // If this clip is already playing.
        if (musicSource.clip == audioClip && musicSource.isPlaying)
        {
            if (!forceRestartClip)
            {
                return;
            }
        }
        musicSource.clip = audioClip;
        musicSource.Play();
        
        if (newVolume >= 0f && newVolume <= 1f)
        {
            musicSource.volume = newVolume;
        }
    }

    public void FadeOutMusic(float duration = 2f)
    {
        StartCoroutine(FadeOut(musicSource, duration));
    }

    public IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        while (musicSource.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    public void FadeInMusic(float duration = 2f)
    {
        StartCoroutine(FadeIn(musicSource, duration));
    }

    public IEnumerator FadeIn(AudioSource source, float duration)
    {
        float targetVolume = source.volume;
        source.volume = 0f;
        source.Play();

        while (source.volume < targetVolume)
        {
            source.volume += targetVolume * Time.deltaTime / duration;
            yield return null;
        }

        source.volume = targetVolume;
    }
    #endregion
}
