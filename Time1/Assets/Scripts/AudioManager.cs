using UnityEngine;
using UnityEngine.Audio; 
using System;
using UnityEngine.SceneManagement;

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

    public AudioSource source;


    public void SetSource(AudioSource audioSource)
    {
        source = audioSource;
        source.clip = clip;
        source.loop = loop;

        source.playOnAwake = false;

        float calculatedVolume = AudioManager.globalVolume * this.volume;
        source.volume = calculatedVolume;
    }

    public void UpdateVolume(float newVolume)
    {
        volume = newVolume;
        if (source != null)
        {
            float modifier = 1 + UnityEngine.Random.Range(-randomVolume / 2f, randomVolume / 2f); 
            source.volume = newVolume * modifier;
        }
    }

    public void Play()
    {
        if (source == null)
        {
            
            return;
        }

        float randomVolumeModifier = 1 + UnityEngine.Random.Range(-randomVolume / 2f, randomVolume / 2f); 
        float randomPitchModifier = 1 + UnityEngine.Random.Range(-randomPitch / 2f, randomPitch / 2f); 

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
            AudioSource newAudioSource = _go.AddComponent<AudioSource>();
            sounds[i].SetSource(newAudioSource);
        }

        SetMusicVolume(globalVolume);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {


        if (scene.name.StartsWith("Capitulo") || scene.name == "Fim" || scene.name == "Escolha")
        {
            StopSound("MenuMusic");
            StopSound("BattleMusic");
            StopSound("Dia1");
            StopSound("Dia2");
            StopSound("Baile");
        }
        else if (scene.name.EndsWith("Playtest") || (scene.name.EndsWith("Final")))
        {
            StopSound("MenuMusic");
            PlayIfNotPlaying("BattleMusic");
        }
        else if (scene.name == "Menu")
        {
            PlayIfNotPlaying("MenuMusic");
            StopSound("BattleMusic");
            StopSound("Dia1");
            StopSound("Dia2");
            StopSound("Baile");
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

    public void SetMusicVolume(float newVolume)
    {

        globalVolume = Mathf.Clamp01(newVolume);


        foreach (Sound s in sounds)
        {
            if (s.source != null && s.source.isPlaying)
            {
                s.source.volume = globalVolume * s.volume;
            }
        }
        
    }

    private void PlayIfNotPlaying(string name)
    {
        foreach (var s in sounds)
        {
            if (s.name == name)
            {
                if (!s.source.isPlaying)
                    s.Play();
                return;
            }
        }
    }
}