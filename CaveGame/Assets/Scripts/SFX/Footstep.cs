using UnityEngine;

/// <summary>
/// Simple component that plays footstep sounds
/// </summary>
public class Footstep : MonoBehaviour
{
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindFirstObjectByType<SoundManager>();
    }

    public void PlayFootstep()
    {
        soundManager.PlaySound(SoundType.PLAYER_FOOSTEP);
    }
}
