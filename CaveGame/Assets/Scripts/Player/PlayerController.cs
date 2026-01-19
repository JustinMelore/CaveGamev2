using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

/// <summary>
/// Script that controls the player, including both movement and the radio
/// </summary>

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;

    [Header("Static settings")]
    [SerializeField] private Transform playerCamera;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;

    [Header("Radio Settings")]
    [SerializeField] private AudioSource radioStaticSource;
    [SerializeField] private AudioSource radioObjectiveSoundSource;
    [SerializeField][UnityEngine.Range(-1, 1)] private float minimumObjectiveCloseness;
    [SerializeField][UnityEngine.Range(0, 1)] private float radioStaticMaxVolume;
    [SerializeField][UnityEngine.Range(0, 1)] private float radioObjectiveMaxVolume;

    [Header("Interaction Settings")]
    [SerializeField][UnityEngine.Range(-1, 1)] private float minimumInteractableCloseness;

    //Events
    public static event Action<InteractionRange> OnLookAtInteractable;
    public static event Action<InteractionRange> OnLookAwayFromInteractable;
    public static event Action<InteractionRange> OnInteractWithObject;
    public static event Action<SoundLevel> OnCauseSound;

    private Vector3 playerVelocity;
    private Vector3 playerRotation;
    private bool runButtonPressed = false;
    private bool isRunning = false;
    private bool isTuning = false;
    private InteractionRange interactable;
    private bool canInteract;
    private bool hidden = false;

    //Made a hash set to easily add and remove objectives
    private HashSet<ObjectiveItem> objectivesInRange;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        objectivesInRange = new HashSet<ObjectiveItem>();
        Cursor.lockState = CursorLockMode.Locked;

        //TODO Uncomment for audio feedback
        radioStaticSource.volume = 0f;
        radioObjectiveSoundSource.volume = 0f;
    }

    private void OnEnable()
    {
        ObjectiveItem.OnObjeciveRangeEnter += AddObjective;
        ObjectiveItem.OnObjectiveRangeExit += RemoveObjective;
        InteractionRange.OnInteractRangeEnter += RegisterInteractable;
        InteractionRange.OnInteractRangeExit += RemoveInteractable;
        HidingPlace.OnHidingEnter += Hide;
        HidingPlace.OnHidingExit += ExitHiding;
    }

    private void OnDisable()
    {
        ObjectiveItem.OnObjeciveRangeEnter -= AddObjective;
        ObjectiveItem.OnObjectiveRangeExit -= RemoveObjective;
        InteractionRange.OnInteractRangeEnter -= RegisterInteractable;
        InteractionRange.OnInteractRangeExit -= RemoveInteractable;
    }

    /// <summary>
    /// Triggers when the player interacts with the sprint button
    /// </summary>
    /// <param name="inputValue"></param>
    private void OnSprint(InputValue inputValue)
    {
        if (!enabled && inputValue.isPressed) return;
        runButtonPressed = inputValue.isPressed;
        //Debug.Log($"{isRunning}");
    }

    /// <summary>
    /// Triggers when the player interacts with the movement buttons
    /// </summary>
    /// <param name="inputValue"></param>
    private void OnMove(InputValue inputValue)
    {
        //Vector2 inputVector = Vector2.Normalize(inputValue.Get<Vector2>());
        Vector2 inputVector = inputValue.Get<Vector2>();
        inputVector.Normalize();
        playerVelocity.x = inputVector.x;
        playerVelocity.z = inputVector.y;
    }

    /// <summary>
    /// Triggers when the camera moves the camera
    /// </summary>
    /// <param name="inputValue"></param>
    private void OnLook(InputValue inputValue)
    {
        Vector2 mouseMovement = inputValue.Get<Vector2>();
        playerRotation.y += mouseMovement.x * sensitivity;
        playerRotation.x += -mouseMovement.y * sensitivity;
        playerRotation.x = Mathf.Clamp(playerRotation.x, -90f, 90f);
    }

    /// <summary>
    /// Triggers when the player interacts with the tune radio button
    /// </summary>
    /// <param name="inputValue"></param>
    private void OnTuneRadio(InputValue inputValue)
    {
        if (!enabled || hidden) return;
        isTuning = inputValue.isPressed;
        animator.SetBool("Tuning", isTuning);
        if (isTuning)
        {

            radioStaticSource.volume = radioStaticMaxVolume;
            Debug.Log("Tuning radio");
        }
        else
        {
            radioStaticSource.volume = 0f;
            radioObjectiveSoundSource.volume = 0f;
            Debug.Log("Stopped tuning radio");
        }
    }

    private void StopTuning()
    {
        animator.SetBool("Tuning", false);
        radioStaticSource.volume = 0f;
        radioObjectiveSoundSource.volume = 0f;
        isTuning = false;
        Debug.Log("Stopped tuning radio");
    }

    /// <summary>
    /// Triggers when the player interacts with the interact button
    /// </summary>
    /// <param name="inputValue"></param>
    private void OnInteract(InputValue inputValue)
    {
        if (!enabled) return;
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
        EmitSound();
    }

    /// <summary>
    /// Helper method that determines what sound level the player is emitting this frame
    /// </summary>
    private void EmitSound()
    {
        if (isTuning) OnCauseSound?.Invoke(SoundLevel.LOUD);
        else if (isRunning) OnCauseSound?.Invoke(SoundLevel.MODERATE);
        else if (IsMoving()) OnCauseSound?.Invoke(SoundLevel.QUIET);
    }

    /// <summary>
    /// Moves the player based on their current inputs
    /// </summary>
    private void MovePlayer()
    {
        if (characterController.isGrounded && playerVelocity.y < 0) playerVelocity.y = -2f;
        playerVelocity.y += gravity * Time.deltaTime;
        if (!enabled) return;
        //transform.rotation = Quaternion.Euler(playerRotation);
        transform.rotation = Quaternion.Euler(0f, playerRotation.y, 0f);
        //Vector3 movement = (runButtonPressed && !isTuning ? sprintSpeedMultiplier : 1f) * moveSpeed * (transform.right * playerVelocity.x + transform.forward * playerVelocity.z);
        Vector3 movement;
        if(runButtonPressed && !isTuning)
        {
            isRunning = true;
            movement = sprintSpeedMultiplier * moveSpeed * (transform.right * playerVelocity.x + transform.forward * playerVelocity.z);
        } else
        {
            isRunning = false;
            movement = moveSpeed * (transform.right * playerVelocity.x + transform.forward * playerVelocity.z);
        }
        movement.y = playerVelocity.y;
        playerCamera.transform.localRotation = Quaternion.Euler(playerRotation.x, 0f, 0f);
        if(!hidden) characterController.Move(movement * Time.deltaTime);


        if (playerVelocity.x != 0 || playerVelocity.z != 0)
        {
            if (isRunning)
            {
                animator.SetInteger("Speed", 2);
                //OnCauseSound?.Invoke(SoundLevel.MODERATE);
            }
            else
            {
                animator.SetInteger("Speed", (runButtonPressed) ? 2 : 1);
                //OnCauseSound?.Invoke(SoundLevel.QUIET);
            }
        } else
        {
            animator.SetInteger("Speed", 0);
        }
    }

    /// <summary>
    /// Checks to see if the player can interact with an object and updates their status based on the result
    /// </summary>
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

    /// <summary>
    /// Determines if the player is moving horizontally
    /// </summary>
    /// <returns></returns>
    public bool IsMoving()
    {
        return playerVelocity.x != 0 || playerVelocity.z != 0;
    }

    /// <summary>
    /// Determines if the player is running
    /// </summary>
    /// <returns></returns>
    public bool IsRunning()
    {
        return IsMoving() && isRunning;
    }

    /// <summary>
    /// Adds a new objective to the list of ones in range of the player
    /// </summary>
    /// <param name="objective"></param>
    private void AddObjective(ObjectiveItem objective)
    {
        objectivesInRange.Add(objective);
        Debug.Log($"Objective in range; {objectivesInRange.Count} objectives in range");
    }

    /// <summary>
    /// Removes a given objective from the list of ones in range of the player
    /// </summary>
    /// <param name="objective"></param>
    private void RemoveObjective(ObjectiveItem objective)
    {
        objectivesInRange.Remove(objective);
        Debug.Log($"Objective out of range; {objectivesInRange.Count} objectives in range");
    }

#nullable enable
    /// <summary>
    /// Calculates what the closest objective to the player is and tunes to it if one exists, with the volume of the objective scaling with look precision and distance
    /// </summary>
    private void ScanForObjectives()
    {
        if (objectivesInRange.Count == 0)
        {
            //TODO Uncomment for audio feedback
            radioStaticSource.volume = radioStaticMaxVolume;
            radioObjectiveSoundSource.volume = 0f;
        }

        float mostDirectDot = -1f;
        ObjectiveItem? mostDirectObjective = null;

        float minimumCloseness = 0f;
        float distancePercentage = 0f;

        foreach (ObjectiveItem objective in objectivesInRange)
        {
            float currentObjectiveDotProduct = CalculateLookCloseness(objective.gameObject);
            float distanceToObjective = Vector3.Distance(objective.transform.position, transform.position);

            distancePercentage = (objective.GetObjectiveRange() - distanceToObjective - 1) / (objective.GetObjectiveRange() - 1);
            //The closer the player is to the objective, the less precisely they have to look at it
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

            //TODO Uncomment for audio feedback
            radioStaticSource.volume = radioStaticMaxVolume - radioStaticMaxVolume * objectiveVolumePercentage;
            radioObjectiveSoundSource.volume = radioObjectiveMaxVolume * objectiveVolumePercentage;
            radioObjectiveSoundSource.volume = radioObjectiveMaxVolume * distancePercentage;
        }
        else
        {
            //TODO Uncomment for audio feedback
            radioStaticSource.volume = radioStaticMaxVolume;
            radioObjectiveSoundSource.volume = 0f;
        }
        //OnCauseSound?.Invoke(SoundLevel.LOUD);
    }

    /// <summary>
    /// Calculates how closely the player is looking at a given object
    /// </summary>
    /// <param name="other">The game object being looked at</param>
    /// <returns></returns>
    private float CalculateLookCloseness(GameObject other)
    {
        Vector3 directionToObject = other.transform.position - transform.position;
        directionToObject.Normalize();
        return Vector3.Dot(directionToObject, transform.forward);
    }

    /// <summary>
    /// Sets a given interactable to be the one the player can currently interact with
    /// </summary>
    /// <param name="interactable"></param>
    private void RegisterInteractable(InteractionRange interactable)
    {
        this.interactable = interactable;
        Debug.Log($"Entered interaction range of {interactable}");
    }

    /// <summary>
    /// Makes the player unable to interact with a given interactable
    /// </summary>
    /// <param name="interactable"></param>
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

    /// <summary>
    /// Hides the player in a given hiding place
    /// </summary>
    /// <param name="hidingPlace"></param>
    private void Hide(HidingPlace hidingPlace)
    {
        //TODO Tweak to accomodate hiding animation
        hidden = true;
        characterController.enabled = false;
        transform.position = hidingPlace.GetEntryPoint().position;
        //playerRotation = Vector3.zero;
        playerRotation = hidingPlace.transform.rotation.eulerAngles;
        playerVelocity = Vector3.zero;
        characterController.enabled = true;
        StopTuning();
        Debug.Log($"Player hiding in {hidingPlace.gameObject}");
    }

    /// <summary>
    /// Has the player stop hiding in a given hiding place
    /// </summary>
    /// <param name="hidingPlace"></param>
    private void ExitHiding(HidingPlace hidingPlace)
    {
        //TODO Tweak to accomodate exiting animation
        hidden = false;
        characterController.enabled = false;
        transform.position = hidingPlace.GetExitPoint().position;
        characterController.enabled = true;
        Debug.Log($"Player exited {hidingPlace.gameObject}");
    }
}