using UnityEngine;

public class Behavior_CollectFood : IAntBehavior
{
    public string BehaviorName => "CollectFood";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        // Pinned ants cannot be interrupted while carrying
        if (ant.carryingFoodAmount > 0 && ant.isPinned) return false;
        return ant.carryingFoodAmount > 0;
    }

    public void OnAssigned(Ant ant)
    {
        if (!ant.isPinned)
        {
            ant.targetFood = null;
        }
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        if (ant.carryingFoodAmount > 0)
        {
            DeliverFood(ant);
            return;
        }

        // If pinned and have a target that's still good, skip finding
        if (ant.isPinned && ant.targetFood != null && !ant.targetFood.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetFood.transform.position);
            if (dist < 0.5f)
            {
                CollectFood(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetFood.transform.position);
            }
            return;
        }

        // If pinned, target is depleted, clear it so ant finds a new source
        if (ant.isPinned && ant.targetFood != null && ant.targetFood.isDepleted)
        {
            ant.targetFood = null;
        }

        if (ant.targetFood == null || ant.targetFood.isDepleted)
        {
            FindFood(ant);
        }

        if (ant.targetFood != null && !ant.targetFood.isDepleted)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetFood.transform.position);
            if (dist < 0.5f)
            {
                CollectFood(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetFood.transform.position);
            }
        }
    }

    public bool IsComplete(Ant ant)
    {
        return ant.carryingFoodAmount == 0 && ant.targetFood == null;
    }

    private void FindFood(Ant ant)
    {
        ant.targetFood = null;
        if (ResourceSpawner.Instance == null) return;

        float bestDist = float.MaxValue;
        float searchRadius = 15f;

        foreach (Food f in ResourceSpawner.Instance.activeFood)
        {
            if (f != null && !f.isDepleted)
            {
                float d = Vector2.Distance(ant.transform.position, f.transform.position);
                if (d <= searchRadius && d < bestDist)
                {
                    Vector2Int gridPos = GridManager.Instance.WorldToGrid(f.transform.position);
                    if (GridManager.Instance.IsWalkable(gridPos))
                    {
                        bestDist = d;
                        ant.targetFood = f;
                    }
                }
            }
        }
    }

    private void CollectFood(Ant ant)
    {
        int taken = ant.targetFood.TakeFood(ant.carryCapacity);
        ant.carryingFoodAmount += taken;
        ant.currentPath = null;
    }

    private void DeliverFood(Ant ant)
    {
        if (ResourceStorage.Instance == null) return;

        Vector2 storagePos = ResourceStorage.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, storagePos);

        if (dist < 1.0f)
        {
            ResourceStorage.Instance.Deposit(ResourceType.Food, ant.carryingFoodAmount);
            ant.carryingFoodAmount = 0;
            ant.currentPath = null;

            // If pinned and target still exists and not depleted, keep looping
            if (ant.isPinned && ant.targetFood != null && !ant.targetFood.isDepleted)
            {
                return;
            }

            // Source depleted or not pinned - clear target and let ant find a new source
            ant.targetFood = null;
        }
        else
        {
            ant.FollowPathTo(storagePos);
        }
    }
}