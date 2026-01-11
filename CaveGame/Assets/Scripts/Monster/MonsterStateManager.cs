using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Master controller of the Monster AI. Responsible for switching between and interacting with the various AI states
/// </summary>
public class MonsterStateManager : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Idling Settings")]
    [SerializeField] private float idleTime;

    [Header("Wandering Settings")]
    [SerializeField] private float wanderSpeed = 1f;
    [SerializeField] private float wanderRadius = 10f;

    [Header("Investigating Settings")]
    //TODO make several different investigating speeds
    [SerializeField] private float investigatingSpeed;


    private MonsterState currentState;

    public WanderingState WanderingState { get; private set; }
    public IdleState IdleState { get; private set; }
    public InvestigatingState InvestigatingState { get; private set; }

    /// <summary>
    /// The position of the last significant sound heard by the monster
    /// </summary>
    public Sound TriggeringSound { get; set; }

    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        IdleState = new IdleState(idleTime);
        WanderingState = new WanderingState(agent, wanderSpeed, wanderRadius);
        InvestigatingState = new InvestigatingState(agent, investigatingSpeed);
    }

    private void Start()
    {
        SwitchState(WanderingState);
    }

    void Update()
    {
        currentState.Update(this);
    }

    /// <summary>
    /// Switches the monster's current state
    /// </summary>
    /// <param name="newState">The state to switch the monster to</param>
    public void SwitchState(MonsterState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }

    /// <summary>
    /// Manages the monster's response for when it heears a sound
    /// </summary>
    /// <param name="volume">The volume level of the sound it heard</param>
    /// <param name="position">The position the sound occurred at</param>
    public void SoundHeard(SoundLevel volume, Vector3 position)
    {
        Debug.Log($"Monster heard {volume} sound at {position}");
        currentState.SoundHeard(this, volume, position);
    }
}
 