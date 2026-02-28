using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script responsible for spawning objectives in the level and keep track of how many have been collected
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private GameObject objective;
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private int objectiveCount;
    private int remainingObjectives;

    private void Awake()
    {
        remainingObjectives = objectiveCount;
    }

    private void Start()
    {
        HashSet<int> usedSpawns = new HashSet<int>(objectiveCount);
        for(int i = 0; i < objectiveCount; i++)
        {
            int currentIndex = 0;
            bool uniqueSpawnFound = false;
            while(!uniqueSpawnFound)
            {
                currentIndex = UnityEngine.Random.Range(0, spawnPositions.Length);
                if (!usedSpawns.Contains(currentIndex)) uniqueSpawnFound = true;
            }
            usedSpawns.Add(currentIndex);
            Instantiate(objective, spawnPositions[currentIndex]);
        }
    }

    private void OnEnable()
    {
        ObjectiveItem.ObjectiveCollectedEvent += OnObjectiveCollected;
    }

    private void OnDisable()
    {
        ObjectiveItem.ObjectiveCollectedEvent -= OnObjectiveCollected;
    }

    private void OnObjectiveCollected()
    {
        remainingObjectives--;
        Debug.Log($"Objective collected. Objectives remaining: {remainingObjectives}");
        //TODO Update wtih proper win condition
        if(remainingObjectives <= 0)
        {
            Debug.Log("All objectives collected!");
        }
    }
}
