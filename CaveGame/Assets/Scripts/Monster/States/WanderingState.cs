using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State for when the monster is wandering around to random locations
/// </summary>
public class WanderingState : MonsterState
{
    private float wanderSpeed;
    private float wanderRadius;

    /// <summary>
    /// Creates a new WanderingState object
    /// </summary>
    /// <param name="agent">The NavMesh Agent associated with the monster</param>
    /// <param name="wanderSpeed">The speed the monster should wander</param>
    public WanderingState(NavMeshAgent agent, float wanderSpeed, float wanderRadius)
    {
        this.agent = agent;
        this.wanderSpeed = wanderSpeed;
        this.wanderRadius = wanderRadius;
    }
    
    /// <summary>
    /// Gets a new random target for the monster to wander to
    /// </summary>
    /// <param name="monsterPosition">The current position of the monster</param>
    /// <returns>The new position for the monster to wander to</returns>
    private Vector3 GetTargetPosition(Vector3 monsterPosition)
    {
        bool isValidTarget = false;
        NavMeshHit hit = new NavMeshHit();
        while(!isValidTarget)
        {
            Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
            Vector3 desiredTarget = monsterPosition + new Vector3(randomDirection.x, 0f, randomDirection.y);
            isValidTarget = NavMesh.SamplePosition(desiredTarget, out hit, 10f, 1);
        }
        return hit.position;
    }

    /// <summary>
    /// Has the monster wander to a new random position
    /// </summary>
    /// <param name="monsterPosition">The monster's current position</param>
    private void WanderToPoint(Vector3 monsterPosition)
    {
        Vector3 wanderPosition = GetTargetPosition(monsterPosition);
        agent.SetDestination(wanderPosition);
        Debug.Log($"Monster wandering to {wanderPosition}");
    }

    public override void EnterState(MonsterStateManager manager)
    {
        agent.speed = wanderSpeed;
        WanderToPoint(manager.transform.position);
    }

    public override void Update(MonsterStateManager manager)
    {
        if(agent.remainingDistance <= 0.1f)
        {
            agent.ResetPath();
            manager.SwitchState(manager.IdleState);
        }
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        agent.ResetPath();
        manager.TriggeringSound = new Sound(volume, position);
        manager.SwitchState(manager.InvestigatingState);
    }
}
