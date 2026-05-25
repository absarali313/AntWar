using System.Collections.Generic;
using UnityEngine;

public class ColonyBlackboard : MonoBehaviour
{
    public static ColonyBlackboard Instance { get; private set; }

    // Resources stored in the colony
    public int foodStored;
    public int waterStored;
    public int mineralsStored;

    // Resources available in the world
    public int foodSourcesInWorld;
    public int waterSourcesInWorld;
    public int mineralSourcesInWorld;

    // Pending tasks
    public int pendingDigTargets;
    public int pendingEggs;

    // Colony census
    public int totalAnts;
    public Dictionary<AntRole, int> antsByRole = new Dictionary<AntRole, int>();
    public Dictionary<string, int> antsByBehavior = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Refresh(List<Ant> allAnts)
    {
        // Read from ResourceStorage
        if (ResourceStorage.Instance != null)
        {
            foodStored = ResourceStorage.Instance.Get(ResourceType.Food);
            waterStored = ResourceStorage.Instance.Get(ResourceType.Water);
            mineralsStored = ResourceStorage.Instance.Get(ResourceType.Minerals);
        }

        // Read from ResourceSpawner
        if (ResourceSpawner.Instance != null)
        {
            foodSourcesInWorld = ResourceSpawner.Instance.activeFood?.Count ?? 0;
            waterSourcesInWorld = ResourceSpawner.Instance.activeWater?.Count ?? 0;
            mineralSourcesInWorld = ResourceSpawner.Instance.activeMinerals?.Count ?? 0;
        }

        // Read from DigCommandManager
        if (DigCommandManager.Instance != null)
        {
            pendingDigTargets = DigCommandManager.Instance.targetCount;
        }

        // Read from EggManager
        if (EggManager.Instance != null)
        {
            pendingEggs = EggManager.Instance.availableEggs?.Count ?? 0;
        }

        // Rebuild census
        totalAnts = allAnts?.Count ?? 0;
        antsByRole.Clear();
        antsByBehavior.Clear();

        foreach (AntRole role in System.Enum.GetValues(typeof(AntRole)))
        {
            antsByRole[role] = 0;
        }

        if (allAnts != null)
        {
            foreach (Ant ant in allAnts)
            {
                if (ant == null) continue;
                antsByRole[ant.role]++;
                string behaviorName = ant.currentBehavior?.BehaviorName ?? "None";
                if (antsByBehavior.ContainsKey(behaviorName))
                    antsByBehavior[behaviorName]++;
                else
                    antsByBehavior[behaviorName] = 1;
            }
        }
    }
}