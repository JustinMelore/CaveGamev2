using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract class representing one state in the monster's AI
/// </summary>
public abstract class MonsterState
{
    protected NavMeshAgent agent;

    /// <summary>
    /// Handles behaviors/events that should occur when the monster enters this state
    /// </summary>
    /// <param name="manager">The state manager interacting with this state</param>
    public abstract void EnterState(MonsterStateManager manager);

    /// <summary>
    /// Handles behavior that should occur each frame the monster is in this state
    /// </summary>
    /// <param name="manager">The state manager interacting with this state</param>
    public abstract void Update(MonsterStateManager manager);

#nullable enable
    /// <summary>
    /// Handles behavior that should occur when the monster hears a sound
    /// </summary>
    /// <param name="volume">The volume level of the sound the monster heard</param>
    /// <param name="position">The position where the sound occurred</param>
    public abstract void SoundHeard(MonsterStateManager manager, SoundLevel volume, Vector3 position);
}
