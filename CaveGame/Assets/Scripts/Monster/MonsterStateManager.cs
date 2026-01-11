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


    private MonsterState currentState;

    public WanderingState WanderingState { get; private set; }
    public IdleState IdleState { get; private set; }

    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        IdleState = new IdleState(idleTime);
        WanderingState = new WanderingState(agent, wanderSpeed, wanderRadius);
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
}
