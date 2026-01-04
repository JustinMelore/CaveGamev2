using UnityEngine;
using System;

/// <summary>
/// Script that handles the range for interactable objects
/// </summary>
public class InteractionRange : MonoBehaviour
{
    public static event Action<InteractionRange> OnInteractRangeEnter;
    public static event Action<InteractionRange> OnInteractRangeExit;

    [SerializeField] private bool isSingleUse = true;

    private void Awake()
    {
        PlayerController.OnInteractWithObject += OnInteract;
    }

    private void OnDisable()
    {
        PlayerController.OnInteractWithObject -= OnInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnInteractRangeEnter?.Invoke(this);
    }

    private void OnTriggerExit(Collider other)
    {
        OnInteractRangeExit?.Invoke(this);
    }

    /// <summary>
    /// Triggers when an object is interacted with by the player
    /// </summary>
    /// <param name="interactable"></param>
    private void OnInteract(InteractionRange interactable)
    {
        if (this == interactable)
        {
            OnInteractRangeExit?.Invoke(this);
            if(isSingleUse)
                gameObject.SetActive(false);
        }
    }
}