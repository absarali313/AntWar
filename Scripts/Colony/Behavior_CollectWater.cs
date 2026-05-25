using UnityEngine;

public class Behavior_CollectWater : IAntBehavior
{
    public string BehaviorName => "CollectWater";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        if (ant.carryingWaterAmount > 0 && ant.isPinned) return false;
        return ant.carryingWaterAmount > 0;
    }

    public void OnAssigned(Ant ant)
    {
        if (!ant.isPinned)
        {
            ant.targetWater = null;
        }
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        if (ant.carryingWaterAmount > 0)
        {
            DeliverWater(ant);
            return;
        }

        // If pinned and have a target that's still good, skip finding
        if (ant.isPinned && ant.targetWater != null && !ant.targetWater.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetWater.transform.position);
            if (dist < 0.5f)
            {
                CollectWater(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetWater.transform.position);
            }
            return;
        }

        // If pinned, target is depleted, clear it so ant finds a new source
        if (ant.isPinned && ant.targetWater != null && ant.targetWater.isDepleted)
        {
            ant.targetWater = null;
        }

        if (ant.targetWater == null || ant.targetWater.isDepleted)
        {
            FindWater(ant);
        }

        if (ant.targetWater != null && !ant.targetWater.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetWater.transform.position);
            if (dist < 0.5f)
            {
                CollectWater(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetWater.transform.position);
            }
        }
    }

    public bool IsComplete(Ant ant)
    {
        return ant.carryingWaterAmount == 0 && ant.targetWater == null;
    }

    private void FindWater(Ant ant)
    {
        ant.targetWater = null;
        if (ResourceSpawner.Instance == null) return;

        float bestDist = float.MaxValue;
        float searchRadius = 15f;

        foreach (WaterSource w in ResourceSpawner.Instance.activeWater)
        {
            if (w != null && !w.isDepleted)
            {
                float d = Vector2.Distance(ant.transform.position, w.transform.position);
                if (d <= searchRadius && d < bestDist)
                {
                    Vector2Int gridPos = GridManager.Instance.WorldToGrid(w.transform.position);
                    if (GridManager.Instance.IsWalkable(gridPos))
                    {
                        bestDist = d;
                        ant.targetWater = w;
                    }
                }
            }
        }
    }

    private void CollectWater(Ant ant)
    {
        int taken = ant.targetWater.TakeWater(ant.carryCapacity);
        ant.carryingWaterAmount += taken;
        ant.currentPath = null;
    }

    private void DeliverWater(Ant ant)
    {
        if (ResourceStorage.Instance == null) return;

        Vector2 storagePos = ResourceStorage.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, storagePos);

        if (dist < 1.0f)
        {
            ResourceStorage.Instance.Deposit(ResourceType.Water, ant.carryingWaterAmount);
            ant.carryingWaterAmount = 0;
            ant.currentPath = null;

            // If pinned and target still exists and not depleted, keep looping
            if (ant.isPinned && ant.targetWater != null && !ant.targetWater.isDepleted)
            {
                return;
            }

            // Source depleted or not pinned - clear target and let ant find a new source
            ant.targetWater = null;
        }
        else
        {
            ant.FollowPathTo(storagePos);
        }
    }
}