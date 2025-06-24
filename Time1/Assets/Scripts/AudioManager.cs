using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0.5f, 1.5f)] public float pitch = 1.0f;
    [Range(0f, 1f)] public float randomVolume = 0.1f;
    [Range(0f, 1f)] public float randomPitch = 0.1f;

    public bool loop = false;

    private AudioSource source;

    public void SetSource(AudioSource audioSource)
    {
        source = audioSource;
        source.clip = clip;
        source.loop = loop;
    }

    public void UpdateVolume(float newVolume)
    {
        volume = newVolume;
        if (source != null)
        {
            float modifier = 1 + Random.Range(-randomVolume / 2f, randomVolume / 2f);
            source.volume = newVolume * modifier;
        }
    }

    public void Play()
    {
        if (source == null)
        {
            Debug.LogWarning("Sound.Play() called but source is null for sound: " + name);
            return;
        }

        float randomVolumeModifier = 1 + Random.Range(-randomVolume / 2f, randomVolume / 2f);
        float randomPitchModifier = 1 + Random.Range(-randomPitch / 2f, randomPitch / 2f);

        source.volume = AudioManager.globalVolume * randomVolumeModifier;
        source.pitch = pitch * randomPitchModifier;
        source.Play();
    }

    public void Stop() { if (source != null) source.Stop(); }
    public void Pause() { if (source != null) source.Pause(); }
    public void UnPause() { if (source != null) source.UnPause(); }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public static float globalVolume = 1f;

    [SerializeField] Sound[] sounds;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            _go.transform.SetParent(this.transform);
            sounds[i].SetSource(_go.AddComponent<AudioSource>());
        }

        SetMusicVolume(globalVolume);
    }

    private void Start()
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].loop)
                sounds[i].Play();
        }
    }

    public void PlaySound(string _name)
    {
        foreach (var s in sounds)
        {
            if (s.name == _name)
            {
                s.Play();
                return;
            }
        }
        Debug.LogWarning("AudioManager: Sound " + _name + " not found!");
    }

    public void StopSound(string _name)
    {
        foreach (var s in sounds)
        {
            if (s.name == _name)
            {
                s.Stop();
                return;
            }
        }
    }

    public void PauseSound(string _name)
    {
        foreach (var s in sounds)
        {
            if (s.name == _name)
            {
                s.Pause();
                return;
            }
        }
    }

    public void UnPauseSound(string _name)
    {
        foreach (var s in sounds)
        {
            if (s.name == _name)
            {
                s.UnPause();
                return;
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        globalVolume = volume;
        foreach (var s in sounds)
        {
            s.UpdateVolume(volume);
        }
    }
}
