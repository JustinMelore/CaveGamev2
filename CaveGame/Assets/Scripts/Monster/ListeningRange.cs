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

    private void OnTriggerEnter(Collider other)
    {
        player = other.transform.GetComponent<PlayerController>();
        PlayerController.OnCauseSound += SoundHeard;
    }

    private void OnTriggerExit(Collider other)
    {
        player = null;
        PlayerController.OnCauseSound -= SoundHeard;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, transform.localScale.x / 2);
    }

    /// <summary>
    /// Process a sound of a given level and decides if the monster should hear it or not
    /// </summary>
    /// <param name="volume">The volume level of the sound that occurred</param>
    private void SoundHeard(SoundLevel volume)
    {
        if (volume < minAudibleLevel) return;
        monster.SoundHeard(volume, player.transform.position);
    }
}
