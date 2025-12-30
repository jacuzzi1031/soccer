using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioClipRefsSO audioRefs;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;   // 强制 2D
        audioSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, 1.3f);
    }
}

