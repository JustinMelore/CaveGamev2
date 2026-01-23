using System;
using UnityEngine;

/// <summary>
/// Script for handling objects the player can hide inside
/// </summary>
public class HidingPlace : MonoBehaviour
{
    public static Action<HidingPlace> OnHidingEnter;
    public static Action<HidingPlace> OnHidingExit;
    
    [SerializeField] private InteractionRange interactable;
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;
    private bool inUse;

    private void Awake()
    {
        PlayerController.OnInteractWithObject += OnHidingPlaceInteract;
        inUse = false;
        Debug.Log($"In use: {inUse}");
    }

    private void OnDestroy()
    {
        PlayerController.OnInteractWithObject -= OnHidingPlaceInteract;
    }

    /// <summary>
    /// Triggers when this hiding place is interacted with by the player
    /// </summary>
    /// <param name="interactable">The interaction range that the player interacted with</param>
    private void OnHidingPlaceInteract(InteractionRange interactable)
    {
        if (interactable != this.interactable) return;
        Debug.Log($"In use: {inUse}");
        if(!inUse)
        {
            Debug.Log("Hiding");
            OnHidingEnter?.Invoke(this);
            inUse = true;
        } else
        {
            Debug.Log("Exiting");
            OnHidingExit?.Invoke(this);
            inUse = false;
        }
    }

    public Transform GetEntryPoint()
    {
        return entryPoint;
    }

    public Transform GetExitPoint()
    {
        return exitPoint;
    }
}
