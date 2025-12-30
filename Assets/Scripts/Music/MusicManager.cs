using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private MusicRefSo musicRefs;

    private AudioSource audioSource;
    private AudioClip currentClip;

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
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        if (currentClip == clip) return;

        currentClip = clip;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        currentClip = null;
    }

    public MusicRefSo Refs => musicRefs;
}