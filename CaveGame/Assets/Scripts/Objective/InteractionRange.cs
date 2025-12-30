using UnityEngine;
using System;

public class InteractionRange : MonoBehaviour
{
    public static event Action<InteractionRange> OnInteractRangeEnter;
    public static event Action<InteractionRange> OnInteractRangeExit;

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

    private void OnInteract(InteractionRange interactable)
    {
        if (this == interactable)
        {
            OnInteractRangeExit?.Invoke(this);
            this.gameObject.SetActive(false);
        }
    }
}