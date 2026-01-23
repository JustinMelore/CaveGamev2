using System.Collections.Generic;
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
    [SerializeField] private float quietInvestigatingSpeed = 1f;
    [SerializeField] private float moderateInvestigatingSpeed = 1f;
    [SerializeField] private float onFindNothingRageGain;

    [Header("Chasing Settings")]
    [SerializeField] private float chasingSpeed = 1f;

    [Header("Rage Settings")]
    [SerializeField] private float maxRageAmount = 100f;
    [SerializeField] private float defaultRageGain = 1f;
    [SerializeField] private float soundThreshold = 10f;
    [SerializeField] private float quietSoundGain = 1f;
    [SerializeField] private float moderatSoundGain = 2f;
    [SerializeField] private float loudSoundGain = 3f;
    [SerializeField] private float listeningTime = 10f;

    private Stack<ListeningRange> rangeStack;
    private MonsterState currentState;
    private float currentRage;

    public WanderingState WanderingState { get; private set; }
    public IdleState IdleState { get; private set; }
    public InvestigatingState InvestigatingState { get; private set; }
    public ChasingState ChasingState { get; private set; }
    public EnragedState EnragedState { get; private set; }

    /// <summary>
    /// The position of the last significant sound heard by the monster
    /// </summary>
    public Sound TriggeringSound { get; set; }

    
    void Awake()
    {
        rangeStack = new Stack<ListeningRange>();
        agent = GetComponent<NavMeshAgent>();
        currentRage = 0f;
        IdleState = new IdleState(idleTime);
        WanderingState = new WanderingState(agent, wanderSpeed, wanderRadius);
        InvestigatingState = new InvestigatingState(agent, quietInvestigatingSpeed, moderateInvestigatingSpeed);
        ChasingState = new ChasingState(agent, chasingSpeed, FindFirstObjectByType<PlayerController>());
        EnragedState = new EnragedState(agent, soundThreshold, quietSoundGain, moderatSoundGain, loudSoundGain, listeningTime);
    }

    private void OnEnable()
    {
        ListeningRange.OnPlayerEnterRange += PushListeningStack;
        ListeningRange.OnPlayerExitRange += PopListeningStack;
    }

    private void OnDisable()
    {
        ListeningRange.OnPlayerEnterRange -= PushListeningStack;
        ListeningRange.OnPlayerExitRange -= PopListeningStack;
    }

    private void PushListeningStack(ListeningRange range)
    {
        rangeStack.Push(range);
    }

    private void PopListeningStack(ListeningRange range)
    {
        if(rangeStack.Count > 0 && rangeStack.Peek() == range)
        {
            rangeStack.Pop();
        }
    }

    private void Start()
    {
        SwitchState(WanderingState);
    }

    void Update()
    {
        currentState.Update(this);
        currentRage += defaultRageGain * Time.deltaTime;
        if (currentRage >= maxRageAmount) OnRageFull();
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
    public void SoundHeard(SoundLevel volume, Vector3 position, ListeningRange range)
    {
        //Debug.Log($"Monster heard {volume} sound at {position}");
        if (range != rangeStack.Peek()) return;
        currentState.SoundHeard(this, volume, position);
    }

    public void OnMonsterFoundNothing()
    {
        currentRage += onFindNothingRageGain;
        if (currentRage >= maxRageAmount) OnRageFull();
    }

    public void ClearRage()
    {
        currentRage = 0f;
    }

    public void OnRageFull()
    {
        currentState.RageFull(this);
    }
}
 