using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State for when the monster is investigating a sound
/// </summary>
public class InvestigatingState : MonsterState
{
    private SoundLevel loudestSound;
    //TODO Implement varying investigating speeds;
    private float investigatingSpeed;

    public InvestigatingState(NavMeshAgent agent, float investigatingSpeed)
    {
        this.agent = agent;
        loudestSound = SoundLevel.QUIET;
        this.investigatingSpeed = investigatingSpeed;
    }
    
    public override void EnterState(MonsterStateManager manager)
    {
        agent.speed = investigatingSpeed;
        agent.SetDestination(manager.TriggeringSound.Position);
        Debug.Log($"Investigating {manager.TriggeringSound.Volume} sound at {manager.TriggeringSound.Position}");
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        if (volume < loudestSound) return;
        agent.ResetPath();
        manager.TriggeringSound = new Sound(volume, position);
        agent.SetDestination(position);
        Debug.Log($"Investigating {volume} sound at {position}");
    }

    public override void Update(MonsterStateManager manager)
    {
        //TODO Possibly modify to make distinct from wandering state
        if (agent.path == null) return;
        if (agent.remainingDistance <= 0.1f)
        {
            agent.ResetPath();
            manager.SwitchState(manager.IdleState);
        }
    }
}
