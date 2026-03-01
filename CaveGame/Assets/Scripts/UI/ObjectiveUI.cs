using TMPro;
using UnityEngine;

/// <summary>
/// Class that handles behavior of the objective UI
/// </summary>
public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Animator animator;
    SoundManager soundManager;
    
    private void Awake()
    {
        FindAnyObjectByType<ObjectiveManager>().ObjectiveCountUpdatedEvent += UpdateObjectiveCount;
        soundManager = FindFirstObjectByType<SoundManager>();
    }

    private void UpdateObjectiveCount(int newCount)
    {
        countText.text = newCount.ToString();
        animator.SetTrigger("ObjectiveUpdated");
    }

    private void PlayBoomSound()
    {
        soundManager.PlaySound(SoundType.BOOM);
    }

    private void PlayBeepSound()
    {
        soundManager.PlaySound(SoundType.BEEP);
    }
}
