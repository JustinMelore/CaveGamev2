using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Master controller of the Monster AI. Responsible for switching between and interacting with the various AI states
/// </summary>
public class MonsterStateManager : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    private NavMeshAgent agent;

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
        IdleState = new IdleState(data.idleTime);
        WanderingState = new WanderingState(agent, data.wanderSpeed, data.wanderRadius);
        InvestigatingState = new InvestigatingState(agent, data.quietInvestigatingSpeed, data.moderateInvestigatingSpeed);
        ChasingState = new ChasingState(agent, data.chasingSpeed, FindFirstObjectByType<PlayerController>());
        EnragedState = new EnragedState(agent, data.soundThreshold, data.quietSoundGain, data.moderatSoundGain, data.loudSoundGain, data.listeningTime, data.teleportRange);
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
        currentRage += data.defaultRageGain * Time.deltaTime;
        if (currentRage >= data.maxRageAmount) OnRageFull();
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
        currentRage += data.onFindNothingRageGain;
        if (currentRage >= data.maxRageAmount) OnRageFull();
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
 