using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Idle Settings")]
    public float idleTime;

    [Header("Wandering Settings")]
    public float wanderSpeed;
    public float wanderRadius;

    [Header("Investigating Settings")]
    public float quietInvestigatingSpeed;
    public float moderateInvestigatingSpeed;
    public float onFindNothingRageGain;

    [Header("Chasing Settings")]
    public float chasingSpeed;

    [Header("Rage Settings")]
    public float maxRageAmount;
    public float defaultRageGain;
    public float soundThreshold;
    public float quietSoundGain;
    public float moderatSoundGain;
    public float loudSoundGain;
    public float listeningTime;
    public float teleportRange;
}
