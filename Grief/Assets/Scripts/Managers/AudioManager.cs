using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private List<AudioSample> audioSamples = new List<AudioSample>();
    
    private Dictionary<string, AudioClip> idAudioClipPairs = new Dictionary<string, AudioClip>();

    private static List<AudioSource> activeAudioSources;

    private AudioSource musicAudioSource;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        musicAudioSource = GetComponent<AudioSource>();
        audioSource = transform.AddComponent<AudioSource>();

        foreach (AudioSample audioSample in audioSamples)
        {
            idAudioClipPairs.Add(audioSample.AudioId, audioSample.AudioClip);
        }
    }

    public void PlaySound(string id)
    {
        if (idAudioClipPairs.TryGetValue(id, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayAudioAt(string id, Vector3 position)
    {

    }

    public void PlayAudioOn(string id, Transform transform)
    {

    }

    public void PlayMusic(string id)
    {
        StopMusic();

        AudioClip clip = idAudioClipPairs[id];

        if (clip == null)
        {
            Debug.LogError($"Music of Id [{id}] does not exist");
            return;
        }

        musicAudioSource.clip = clip;

        musicAudioSource.Play();
    }

    public void PauseAllSounds(bool pause)
    {
        if (pause)
        {
            foreach (AudioSource audioSource in activeAudioSources)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
            }
        } 
        else
        {
            foreach (AudioSource audioSource in activeAudioSources)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
            }
        }
    }

    public void PauseMusic(bool pause)
    {
        if (pause && musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
        } 
        else if (!musicAudioSource.isPlaying)
        {
            musicAudioSource.UnPause();
        }
    }

    public void StopAllSounds()
    {
        foreach (AudioSource audioSource in activeAudioSources)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }
}

[Serializable]
public class AudioSample
{
    [SerializeField] private string audioId;
    [SerializeField] private AudioClip audioClip;

    public string AudioId { get { return audioId; } }
    public AudioClip AudioClip { get { return audioClip; } }
}