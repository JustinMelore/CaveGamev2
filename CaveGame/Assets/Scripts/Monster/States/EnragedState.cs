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

    private float currentListeningTime;
    private float currentListeningAmount;

    public EnragedState(NavMeshAgent agent, float soundThreshold, float quietSoundGain, float moderateSoundGain, float loudSoundGain, float listeningTime)
    {
        this.agent = agent;
        this.soundThreshold = soundThreshold;
        this.quietSoundGain = quietSoundGain;
        this.moderateSoundGain = moderateSoundGain;
        this.loudSoundGain = loudSoundGain;
        this.listeningTime = listeningTime;
        currentListeningTime = 0f;
        currentListeningAmount = 0f;
    }

    public override void EnterState(MonsterStateManager manager)
    {
        Debug.Log("Monster enraged and searching for player");
        currentListeningAmount = 0f;
        currentListeningTime = 0f;
        PlayerController.OnCauseSound += SoundHeardGlobal;
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

    public override void Update(MonsterStateManager manager)
    {
        currentListeningTime += Time.deltaTime;
        Debug.Log($"Current listening amount: {currentListeningAmount} / {soundThreshold}");
        if(currentListeningTime >= listeningTime && currentListeningAmount < soundThreshold)
        {
            manager.ClearRage();
            PlayerController.OnCauseSound -= SoundHeardGlobal;
            manager.SwitchState(manager.IdleState);
        } else if(currentListeningAmount >= soundThreshold)
        {
            //TODO Implement teleporting near player
            Debug.Log("Monster found player");
            PlayerController.OnCauseSound -= SoundHeardGlobal;
            manager.SwitchState(manager.ChasingState);
        }
    }
}
