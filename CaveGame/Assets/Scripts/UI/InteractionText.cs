using UnityEngine;

/// <summary>
/// Handles behavior of the UI text that displays when in range of an interactable
/// </summary>
public class InteractionText : MonoBehaviour
{
    [SerializeField] private CanvasGroup display;

    private void Awake()
    {
        display.alpha = 0f;
        PlayerController.OnLookAtInteractable += DisplayText;
        PlayerController.OnLookAwayFromInteractable += HideText;
    }

    private void OnDestroy()
    {
        PlayerController.OnLookAtInteractable -= DisplayText;
        PlayerController.OnLookAwayFromInteractable -= HideText;
    }

    private void DisplayText(InteractionRange interctable)
    {
        display.alpha = 1f;
    }

    private void HideText(InteractionRange interactable)
    {
        display.alpha = 0f;
    }
}
