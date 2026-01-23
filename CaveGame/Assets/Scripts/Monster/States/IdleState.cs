using UnityEngine;

/// <summary>
/// State for when the monster is standing still
/// </summary>
public class IdleState : MonsterState
{
    private float idleTime;
    private float currentIdleTime;
    
    public IdleState(float idleTime)
    {
        currentIdleTime = 0f;
        this.idleTime = idleTime;
    }

    public override void EnterState(MonsterStateManager manager)
    {
        currentIdleTime = 0f;
        Debug.Log("Monster idling");
    }

    public override void RageFull(MonsterStateManager manager)
    {
        //TODO Change to rage state
        Debug.Log("Monster is now enraged");
    }

    public override void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position)
    {
        manager.TriggeringSound = new Sound(volume, position);
        manager.SwitchState(manager.InvestigatingState);
    }

    public override void Update(MonsterStateManager manager)
    {
        currentIdleTime += Time.deltaTime;
        if(currentIdleTime >= idleTime)
        {
            manager.SwitchState(manager.WanderingState);
        }
    }
}
