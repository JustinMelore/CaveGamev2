using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State for when the monster is chasing the player
/// </summary>
public class ChasingState : MonsterState
{
    private float chasingSpeed;
    private PlayerController player;

    private const float targetChangeCooldown = 0.3f;
    private float currentTargetChangeTimer;

    public ChasingState(NavMeshAgent agent, float chasingSpeed, PlayerController player)
    {
        this.agent = agent;
        this.chasingSpeed = chasingSpeed;
        this.player = player;
        currentTargetChangeTimer = targetChangeCooldown;
    }

    public override void EnterState(MonsterStateManager manager)
    {
        //TODO Add logic for playing some kind of triggered animation
        currentTargetChangeTimer = targetChangeCooldown;
        agent.speed = chasingSpeed;
        UpdateTarget();
        Debug.Log("Monster is now chasing the player");
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        //No behavior for hearing a sound
    }

    public override void Update(MonsterStateManager manager)
    {
        currentTargetChangeTimer += Time.deltaTime;
        UpdateTarget();
        if(!agent.pathPending && agent.remainingDistance <= 0.1f)
        {
            agent.ResetPath();
            manager.SwitchState(manager.IdleState);
        }
    }

    /// <summary>
    /// Helper method that updates the monster's current target based on the player's location
    /// </summary>
    private void UpdateTarget()
    {
        if (currentTargetChangeTimer < targetChangeCooldown) return;
        NavMeshHit hit = new NavMeshHit();
        bool isValidTarget = NavMesh.SamplePosition(player.transform.position, out hit, 10f, 1);
        if(isValidTarget)
        {
            agent.SetDestination(hit.position);
            currentTargetChangeTimer = 0f;
        }
    }
}
