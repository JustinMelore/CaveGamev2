using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State for when the monster is investigating a sound
/// </summary>
public class InvestigatingState : MonsterState
{
    private SoundLevel loudestSound;
    private float quietInvestigatingSpeed;
    private float moderateInvestgiatingSpeed;

    private const float targetChangeCooldown = 0.5f;
    private float currentTargetChangeTimer;

    public InvestigatingState(NavMeshAgent agent, float quietInvestigatingSpeed, float moderateInvestigatingSpeed)
    {
        this.agent = agent;
        loudestSound = SoundLevel.NONE;
        this.quietInvestigatingSpeed = quietInvestigatingSpeed;
        this.moderateInvestgiatingSpeed = moderateInvestigatingSpeed;
        currentTargetChangeTimer = targetChangeCooldown;
    }
    
    public override void EnterState(MonsterStateManager manager)
    {
        loudestSound = SoundLevel.NONE;
        currentTargetChangeTimer = targetChangeCooldown;
        HandleNewSound(manager.TriggeringSound, manager);
    }

    /// <summary>
    /// Helper method that handles what should happen when the monster hears a new sound
    /// </summary>
    /// <param name="triggeringSound">The sound that was heard</param>
    /// <param name="manager">The state manager interacting with this state</param>
    private void HandleNewSound(Sound triggeringSound, MonsterStateManager manager)
    {
        if (triggeringSound.Volume < loudestSound) return;

        if (currentTargetChangeTimer < targetChangeCooldown) return;
        currentTargetChangeTimer = 0f;

        manager.TriggeringSound = triggeringSound; //This and the line below are for when this method is used to override an existing sound
        agent.ResetPath();
        loudestSound = triggeringSound.Volume;
        //TODO Add logic for playing a new "triggered" animation when a sound surpasses the previously loudest sound level
        switch (triggeringSound.Volume)
        {
            case SoundLevel.QUIET:
                agent.speed = quietInvestigatingSpeed;
                break;
            case SoundLevel.MODERATE:
                agent.speed = moderateInvestgiatingSpeed;
                break;
            case SoundLevel.LOUD:
                agent.ResetPath();
                manager.SwitchState(manager.ChasingState);
                break;
        }
        NavMeshHit hit = new NavMeshHit();
        NavMesh.SamplePosition(triggeringSound.Position, out hit, 10f, 1);
        agent.SetDestination(hit.position);
        Debug.Log($"Investigating {triggeringSound.Volume} sound at {triggeringSound.Position}");
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        HandleNewSound(new Sound(volume, position), manager);
    }

    public override void Update(MonsterStateManager manager)
    {
        currentTargetChangeTimer += Time.deltaTime;
        if(!agent.pathPending && agent.remainingDistance <= 0.1f)
        {
            agent.ResetPath();
            manager.OnMonsterFoundNothing();
            manager.SwitchState(manager.IdleState);
        }
    }

    public override void RageFull(MonsterStateManager manager)
    {
        //TODO Change to rage state
        Debug.Log("Monster is now enraged");
    }
}
