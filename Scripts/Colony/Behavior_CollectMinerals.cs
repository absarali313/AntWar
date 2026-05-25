using UnityEngine;

public class Behavior_CollectMinerals : IAntBehavior
{
    public string BehaviorName => "CollectMinerals";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        if (ant.carryingMineralAmount > 0 && ant.isPinned) return false;
        return ant.carryingMineralAmount > 0;
    }

    public void OnAssigned(Ant ant)
    {
        if (!ant.isPinned)
        {
            ant.targetMineral = null;
        }
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        if (ant.carryingMineralAmount > 0)
        {
            DeliverMineral(ant);
            return;
        }

        // If pinned and have a target that's still good, skip finding
        if (ant.isPinned && ant.targetMineral != null && !ant.targetMineral.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetMineral.transform.position);
            if (dist < 0.5f)
            {
                CollectMineral(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetMineral.transform.position);
            }
            return;
        }

        // If pinned, target is depleted, clear it so ant finds a new source
        if (ant.isPinned && ant.targetMineral != null && ant.targetMineral.isDepleted)
        {
            ant.targetMineral = null;
        }

        if (ant.targetMineral == null || ant.targetMineral.isDepleted)
        {
            FindMineral(ant);
        }

        if (ant.targetMineral != null && !ant.targetMineral.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetMineral.transform.position);
            if (dist < 0.5f)
            {
                CollectMineral(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetMineral.transform.position);
            }
        }
    }

    public bool IsComplete(Ant ant)
    {
        return ant.carryingMineralAmount == 0 && ant.targetMineral == null;
    }

    private void FindMineral(Ant ant)
    {
        ant.targetMineral = null;
        if (ResourceSpawner.Instance == null) return;

        float bestDist = float.MaxValue;
        float searchRadius = 15f;

        foreach (MineralDeposit m in ResourceSpawner.Instance.activeMinerals)
        {
            if (m != null && !m.isDepleted)
            {
                float d = Vector2.Distance(ant.transform.position, m.transform.position);
                if (d <= searchRadius && d < bestDist)
                {
                    Vector2Int gridPos = GridManager.Instance.WorldToGrid(m.transform.position);
                    if (GridManager.Instance.IsWalkable(gridPos))
                    {
                        bestDist = d;
                        ant.targetMineral = m;
                    }
                }
            }
        }
    }

    private void CollectMineral(Ant ant)
    {
        int taken = ant.targetMineral.TakeMinerals(ant.carryCapacity);
        ant.carryingMineralAmount += taken;
        ant.currentPath = null;
    }

    private void DeliverMineral(Ant ant)
    {
        if (ResourceStorage.Instance == null) return;

        Vector2 storagePos = ResourceStorage.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, storagePos);

        if (dist < 1.0f)
        {
            ResourceStorage.Instance.Deposit(ResourceType.Minerals, ant.carryingMineralAmount);
            ant.carryingMineralAmount = 0;
            ant.currentPath = null;

            // If pinned and target still exists and not depleted, keep looping
            if (ant.isPinned && ant.targetMineral != null && !ant.targetMineral.isDepleted)
            {
                return;
            }

            // Source depleted or not pinned - clear target and let ant find a new source
            ant.targetMineral = null;
        }
        else
        {
            ant.FollowPathTo(storagePos);
        }
    }
}