using System;
using UnityEngine;

/// <summary>
/// Script for the range at which the monster can hear certain sounds
/// </summary>
public class ListeningRange : MonoBehaviour
{
    [SerializeField] private MonsterStateManager monster;
    [SerializeField] private SoundLevel minAudibleLevel;
    [SerializeField] private Color gizmoColor;
    private PlayerController? player;

    public static Action<ListeningRange> OnPlayerEnterRange;
    public static Action<ListeningRange> OnPlayerExitRange;

    private void OnTriggerEnter(Collider other)
    {
        player = other.transform.GetComponent<PlayerController>();
        PlayerController.OnCauseSound += SoundHeard;
        OnPlayerEnterRange?.Invoke(this);
    }

    private void OnTriggerExit(Collider other)
    {
        player = null;
        OnPlayerExitRange?.Invoke(this);
        PlayerController.OnCauseSound -= SoundHeard;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, transform.localScale.x / 2);
    }

    /// <summary>
    /// Processes a sound of a given level and decides if the monster should hear it or not
    /// </summary>
    /// <param name="volume">The volume level of the sound that occurred</param>
    private void SoundHeard(SoundLevel volume)
    {
        if (volume < minAudibleLevel) return;
        //Debug.Log($"{volume} sound adjusted with {minAudibleLevel} = {(int)volume - (int)minAudibleLevel + 1}");
        int adjustedVolume = (int)volume - (int)minAudibleLevel + 1; //The lower the minAudibleLevel is, the more aggressively the monster will react to various audio levels
        monster.SoundHeard((SoundLevel)adjustedVolume, player.transform.position, this);
    }
}
