using System;
using UnityEngine;

/// <summary>
/// Script responsible for playing various sounds in the game
/// </summary>
[ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// Simple helper class that stores infomration aboutn a sound effect
    /// </summary>
    [Serializable]
    private class SoundEffect
    {
        [HideInInspector] public string name;
        public float volume;
        public AudioClip[] audioClips;

        public AudioClip GetRandomClip()
        {
            return audioClips[UnityEngine.Random.Range(0, audioClips.Length)];
        }
    }

    [SerializeField] private AudioSource source;
    [SerializeField] private SoundEffect[] sounds;

    /// <summary>
    /// Plays the sound associated with a given SoundType
    /// </summary>
    /// <param name="sound">The sound to play</param>
    public void PlaySound(SoundType sound)
    {
        SoundEffect soundEffect = sounds[(int)sound];
        source.PlayOneShot(soundEffect.GetRandomClip(), soundEffect.volume);
    }

    //QoL script that makes it easier to edit sound effects in the inspector
#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] soundNames = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref sounds, soundNames.Length);
        for(int i = 0; i < sounds.Length; i++)
        {
            sounds[i].name = soundNames[i];
        } 
    }
#endif

}
