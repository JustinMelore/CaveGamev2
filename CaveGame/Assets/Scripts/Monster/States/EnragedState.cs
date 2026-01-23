using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// State for when the monster becomes enraged and starts listening map-wide for the player
/// </summary>
public class EnragedState : MonsterState
{
    private float soundThreshold;
    private float quietSoundGain;
    private float moderateSoundGain;
    private float loudSoundGain;
    private float listeningTime;
    private float teleportRange;

    private float currentListeningTime;
    private float currentListeningAmount;
    private bool foundPlayer;
    private PlayerController player;

    public EnragedState(NavMeshAgent agent, float soundThreshold, float quietSoundGain, float moderateSoundGain, float loudSoundGain, float listeningTime, float teleportRange)
    {
        this.agent = agent;
        this.soundThreshold = soundThreshold;
        this.quietSoundGain = quietSoundGain;
        this.moderateSoundGain = moderateSoundGain;
        this.loudSoundGain = loudSoundGain;
        this.listeningTime = listeningTime;
        this.teleportRange = teleportRange;

        currentListeningTime = 0f;
        currentListeningAmount = 0f;
        foundPlayer = false;
        player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
    }

    public override void EnterState(MonsterStateManager manager)
    {
        Debug.Log("Monster enraged and searching for player");
        currentListeningAmount = 0f;
        currentListeningTime = 0f;
        PlayerController.OnCauseSound += SoundHeardGlobal;
        foundPlayer = false;
    }


    public override void RageFull(MonsterStateManager manager)
    {
        //Already enraged, no need to handle this
    }

    /// <summary>
    /// Runs when the monster hears a sound while in rage mode
    /// </summary>
    /// <param name="volume">The volume of the sound that was heard</param>
    private void SoundHeardGlobal(SoundLevel volume)
    {
        switch(volume)
        {
            case SoundLevel.QUIET:
                currentListeningAmount += quietSoundGain * Time.deltaTime;
                break;
            case SoundLevel.MODERATE:
                currentListeningAmount += moderateSoundGain * Time.deltaTime;
                break;
            case SoundLevel.LOUD:
                currentListeningAmount += loudSoundGain * Time.deltaTime;
                break;
        }
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        //Not used, as the monster does not use the listening radius objects during this state
    }

    private void OnFoundPlayer(MonsterStateManager manager)
    {
        NavMeshHit hit = new NavMeshHit();
        Vector2 randomDirection = Random.insideUnitCircle * teleportRange;
        Vector3 desiredTarget = player.transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y);
        bool canMoveToTarget = NavMesh.SamplePosition(desiredTarget, out hit, 10f, 1);
        if(canMoveToTarget)
        {
            Debug.Log("Monster found player");
            manager.transform.position = hit.position;
            manager.SwitchState(manager.ChasingState);
        }
        
    }

    public override void Update(MonsterStateManager manager)
    {
        currentListeningTime += Time.deltaTime;
        Debug.Log($"Current listening amount: {currentListeningAmount} / {soundThreshold}");
        if(currentListeningTime >= listeningTime && currentListeningAmount < soundThreshold)
        {
            manager.ClearRage();
            PlayerController.OnCauseSound -= SoundHeardGlobal;
            manager.SwitchState(manager.IdleState);
        } else if(currentListeningAmount >= soundThreshold && !foundPlayer)
        {
            foundPlayer = true;
            PlayerController.OnCauseSound -= SoundHeardGlobal;
        } else if(foundPlayer)
        {
            OnFoundPlayer(manager);
        }
    }
}
