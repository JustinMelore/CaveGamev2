using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Camera playerCamera;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;

    [Header("Radio Settings")]
    //[SerializeField] private AudioSource radioStaticSource;
    //[SerializeField] private AudioSource radioObjectiveSoundSource;
    [SerializeField][UnityEngine.Range(-1, 1)] private float minimumObjectiveCloseness;
    [SerializeField][UnityEngine.Range(0, 1)] private float radioStaticMaxVolume;
    [SerializeField][UnityEngine.Range(0, 1)] private float radioObjectiveMaxVolume;

    [Header("Interaction Settings")]
    [SerializeField][UnityEngine.Range(-1, 1)] private float minimumInteractableCloseness;

    public static event Action<InteractionRange> OnLookAtInteractable;
    public static event Action<InteractionRange> OnLookAwayFromInteractable;
    public static event Action<InteractionRange> OnInteractWithObject;

    private Vector3 playerVelocity;
    private Vector3 playerRotation;
    private bool isRunning = false;
    private bool isTuning = false;
    private InteractionRange interactable;
    private bool canInteract;

    //Made a hash set to easily add and remove objectives
    private HashSet<ObjectiveItem> objectivesInRange;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = FindFirstObjectByType<Camera>();
        objectivesInRange = new HashSet<ObjectiveItem>();
        ObjectiveItem.OnObjeciveRangeEnter += AddObjective;
        ObjectiveItem.OnObjectiveRangeExit += RemoveObjective;
        InteractionRange.OnInteractRangeEnter += RegisterInteractable;
        InteractionRange.OnInteractRangeExit += RemoveInteractable;
        //radioStaticSource.volume = 0f;
        //radioObjectiveSoundSource.volume = 0f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //public Vector3 getPV() //added to make creature tracking easier
    //{
    //    return playerVelocity;
    //}

    //public AudioSource getRSS() //same as above
    //{
    //    return radioStaticSource;
    //}

    //public AudioSource getROS() //same as above
    //{
    //    return radioObjectiveSoundSource;
    //}

    private void OnSprint(InputValue inputValue)
    {
        isRunning = inputValue.isPressed;
        Debug.Log($"{isRunning}");
    }

    private void OnMove(InputValue inputValue)
    {
        //Vector2 inputVector = Vector2.Normalize(inputValue.Get<Vector2>());
        Vector2 inputVector = inputValue.Get<Vector2>();
        inputValue.Get<Vector2>().Normalize();
        playerVelocity.x = inputVector.x;
        playerVelocity.z = inputVector.y;
    }

    private void OnLook(InputValue inputValue)
    {
        Vector2 mouseMovement = inputValue.Get<Vector2>();
        playerRotation.y += mouseMovement.x * sensitivity;
        playerRotation.x += -mouseMovement.y * sensitivity;
        playerRotation.x = Mathf.Clamp(playerRotation.x, -90f, 90f);
    }

    private void OnTuneRadio(InputValue inputValue)
    {
        isTuning = inputValue.isPressed;
        if (isTuning)
        {

            //radioStaticSource.volume = radioStaticMaxVolume;
            Debug.Log("Tuning radio");
        }
        else
        {
            //radioStaticSource.volume = 0f;
            //radioObjectiveSoundSource.volume = 0f;
            Debug.Log("Stopped tuning radio");
        }
    }

    private void OnInteract(InputValue inputValue)
    {
        if (canInteract && interactable != null)
        {
            OnInteractWithObject?.Invoke(interactable);
            Debug.Log($"Interacted with {interactable}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        if (isTuning) ScanForObjectives();
        if (interactable != null) CheckForInteraction();
    }

    private void MovePlayer()
    {
        if (characterController.isGrounded && playerVelocity.y < 0) playerVelocity.y = -2f;
        playerVelocity.y += gravity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(playerRotation);
        transform.rotation = Quaternion.Euler(0f, playerRotation.y, 0f);
        Vector3 movement = (isRunning && !isTuning ? sprintSpeedMultiplier : 1f) * moveSpeed * (transform.right * playerVelocity.x + transform.forward * playerVelocity.z);
        movement.y = playerVelocity.y;
        playerCamera.transform.localRotation = Quaternion.Euler(playerRotation.x, 0f, 0f);
        characterController.Move(movement * Time.deltaTime);
    }

    private void CheckForInteraction()
    {
        float interactableLookCloseness = CalculateLookCloseness(interactable.gameObject);
        if (interactableLookCloseness >= minimumInteractableCloseness)
        {
            if (!canInteract)
            {
                OnLookAtInteractable?.Invoke(interactable);
                Debug.Log($"Now looking at {interactable}");
            }
            canInteract = true;
        }
        else if (canInteract)
        {
            OnLookAwayFromInteractable?.Invoke(interactable);
            canInteract = false;
            Debug.Log($"Looked away from {interactable}");
        }
    }

    public bool IsMoving()
    {
        return playerVelocity.x != 0 || playerVelocity.z != 0;
    }

    public bool IsRunning()
    {
        return IsMoving() && isRunning;
    }

    private void OnDisable()
    {
        ObjectiveItem.OnObjeciveRangeEnter -= AddObjective;
        ObjectiveItem.OnObjectiveRangeExit -= RemoveObjective;
    }

    private void AddObjective(ObjectiveItem objective)
    {
        objectivesInRange.Add(objective);
        Debug.Log($"Objective in range; {objectivesInRange.Count} objectives in range");
    }

    private void RemoveObjective(ObjectiveItem objective)
    {
        objectivesInRange.Remove(objective);
        Debug.Log($"Objective out of range; {objectivesInRange.Count} objectives in range");
    }

#nullable enable
    private void ScanForObjectives()
    {
        if (objectivesInRange.Count == 0)
        {
            //radioStaticSource.volume = radioStaticMaxVolume;
            //radioObjectiveSoundSource.volume = 0f;
        }
        //float mostDirectDot = minimumObjectiveCloseness;
        float mostDirectDot = -1f;
        ObjectiveItem? mostDirectObjective = null;
        float minimumCloseness = 0f;
        float distancePercentage = 0f;
        foreach (ObjectiveItem objective in objectivesInRange)
        {
            //Vector3 directionToObjective = objective.transform.position - transform.position;
            //directionToObjective.Normalize();
            //float currentObjectiveDotProduct = Vector3.Dot(directionToObjective, transform.forward);
            float currentObjectiveDotProduct = CalculateLookCloseness(objective.gameObject);
            float distanceToObjective = Vector3.Distance(objective.transform.position, transform.position);
            distancePercentage = (objective.GetObjectiveRange() - distanceToObjective - 1) / (objective.GetObjectiveRange() - 1);
            minimumCloseness = minimumObjectiveCloseness - distancePercentage * minimumObjectiveCloseness;
            if (currentObjectiveDotProduct >= mostDirectDot && currentObjectiveDotProduct >= minimumCloseness)
            {
                mostDirectDot = currentObjectiveDotProduct;
                mostDirectObjective = objective;
            }
        }
        if (mostDirectObjective != null)
        {
            float closenessRange = 1f - minimumCloseness;
            mostDirectDot = Mathf.Round(mostDirectDot * 100) / 100 - minimumCloseness;
            Debug.Log($"Closeness range: {closenessRange}");
            Debug.Log($"Closeness: {mostDirectDot}");
            float objectiveVolumePercentage = mostDirectDot / closenessRange;
            //radioStaticSource.volume = radioStaticMaxVolume - radioStaticMaxVolume * objectiveVolumePercentage;
            //radioObjectiveSoundSource.volume = radioObjectiveMaxVolume * objectiveVolumePercentage;
            //radioObjectiveSoundSource.volume = radioObjectiveMaxVolume * distancePercentage;
        }
        else
        {
            //radioStaticSource.volume = radioStaticMaxVolume;
            //radioObjectiveSoundSource.volume = 0f;
        }
    }

    private float CalculateLookCloseness(GameObject other)
    {
        Vector3 directionToObject = other.transform.position - transform.position;
        directionToObject.Normalize();
        return Vector3.Dot(directionToObject, transform.forward);
    }

    private void RegisterInteractable(InteractionRange interactable)
    {
        this.interactable = interactable;
        Debug.Log($"Entered interaction range of {interactable}");
    }

    private void RemoveInteractable(InteractionRange interactable)
    {
        this.interactable = null;
        if (canInteract)
        {
            OnLookAwayFromInteractable?.Invoke(interactable);
            canInteract = false;
        }
        Debug.Log($"Exited interaction range of {interactable}");
    }
}