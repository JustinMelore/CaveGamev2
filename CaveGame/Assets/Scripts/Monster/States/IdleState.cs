using UnityEngine;

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

    public override void Update(MonsterStateManager manager)
    {
        currentIdleTime += Time.deltaTime;
        if(currentIdleTime >= idleTime)
        {
            manager.SwitchState(manager.WanderingState);
        }
    }
}
