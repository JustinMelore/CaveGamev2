using System;
using UnityEngine;
/// <summary>
/// Simple helper class that stores infomration aboutn a sound effect
/// </summary>
[Serializable]
public class SoundEffect
{
    [HideInInspector] public string name;
    [SerializeField] private float volume;
    [SerializeField] private AudioClip[] audioClips;
    public float Volume { get { return volume; } private set { volume = value; } }
    public AudioClip[] AudioClips { get { return audioClips; } private set { audioClips = value; } }

    public AudioClip GetRandomClip()
    {
        return AudioClips[UnityEngine.Random.Range(0, AudioClips.Length)];
    }
}