using System;
using UnityEngine;

public class ObjectiveItem : MonoBehaviour
{
    public static event Action<ObjectiveItem> OnObjeciveRangeEnter;
    public static event Action<ObjectiveItem> OnObjectiveRangeExit;

    private SphereCollider detectionRange;
    [SerializeField] private InteractionRange interactionRange;

    private void Awake()
    {
        PlayerController.OnInteractWithObject += InteractWithObjective;
        detectionRange = GetComponent<SphereCollider>();
    }

    private void OnDisable()
    {
        PlayerController.OnInteractWithObject -= InteractWithObjective;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnObjeciveRangeEnter?.Invoke(this);
    }

    private void OnTriggerExit(Collider other)
    {
        OnObjectiveRangeExit?.Invoke(this);
    }

    public float GetObjectiveRange()
    {
        return detectionRange.radius;
    }

    private void InteractWithObjective(InteractionRange interactable)
    {
        if (interactable == interactionRange)
        {
            //detectionRange.gameObject.SetActive(false);

            PlayerController.OnInteractWithObject -= InteractWithObjective;
            OnObjectiveRangeExit?.Invoke(this);
            //Will be replaced with a different visual indicator in the future, like an interact animation
            transform.parent.GetComponentInChildren<Renderer>().material.color = new Color(0f, 1f, 0f);
            Debug.Log($"Objective collected");
            gameObject.SetActive(false);
        }
    }
}