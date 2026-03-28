using System;
using UnityEngine;

/// <summary>
/// Script responsible for playing various sounds in the game
/// </summary>
[ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private SoundLibrary sounds;

    /// <summary>
    /// Plays the sound associated with a given SoundType
    /// </summary>
    /// <param name="sound">The sound to play</param>
    public void PlaySound(SoundType sound)
    {
        //SoundEffect soundEffect = sounds[(int)sound];
        SoundEffect soundEffect = sounds.soundList[(int)sound];
        source.PlayOneShot(soundEffect.GetRandomClip(), soundEffect.Volume);
    }
}
