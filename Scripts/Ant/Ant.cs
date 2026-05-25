using UnityEngine;
using System.Collections.Generic;

public class Ant : MonoBehaviour
{
    public AntRole role = AntRole.Worker;
    public WorkerJob displayJob = WorkerJob.Idle;
    
    [Header("Collection Duty")]
    public ResourceType assignedDuty = ResourceType.Food;

    public float speed = 2f;
    public float visionRadius = 3f;

    // Pathfinding - made public for behaviors
    public List<Vector2> currentPath;
    public int pathIndex;
    public float pathCooldown = 0f;
    public float pathRefreshRate = 1f;

    // Per-ant stagger offset
    public int idOffset;

    // Wander
    public Vector2 wanderTarget;
    public float wanderTimer = 0f;

    // Dig timing
    public float digCooldown = 0f;
    public float digRate = 0.4f;

    // Food collection
    public Food targetFood;
    public int carryingFoodAmount = 0;
    public int carryCapacity = 1;

    // Water collection
    public WaterSource targetWater;
    public int carryingWaterAmount = 0;

    // Mineral collection
    public MineralDeposit targetMineral;
    public int carryingMineralAmount = 0;

    // Egg carrying
    public Egg carriedEgg;
    public Egg targetEgg;

    // Behavior system
    public IAntBehavior currentBehavior;
    public bool wantsToReturn = false;
    public bool isPinned = false;

    void Start()
    {
        idOffset = Random.Range(0, 60);
        ColonyBrain.Instance?.RegisterAnt(this);
        SetBehavior(new Behavior_Idle());
    }

    void Update()
    {
        if ((Time.frameCount + idOffset) % 6 == 0)
        {
            UpdateVision();
        }

        currentBehavior?.Tick(this);
    }

    void OnDestroy()
    {
        ColonyBrain.Instance?.UnregisterAnt(this);
        if (DigCommandManager.Instance != null)
        {
            DigCommandManager.ReleaseAnt(this);
        }
    }

    public void SetBehavior(IAntBehavior newBehavior)
    {
        if (currentBehavior?.BehaviorName == newBehavior.BehaviorName) return;
        currentBehavior = newBehavior;
        currentBehavior.OnAssigned(this);
        displayJob = MapBehaviorNameToEnum(newBehavior.BehaviorName);
    }

    public WorkerJob MapBehaviorNameToEnum(string name)
    {
        switch (name)
        {
            case "Idle": return WorkerJob.Idle;
            case "Dig": return WorkerJob.Digger;
            case "Returning": return WorkerJob.Returning;
            case "CollectFood": return WorkerJob.FoodCollector;
            case "CarryEgg": return WorkerJob.EggCarrier;
            case "CollectWater": return WorkerJob.WaterCollector;
            case "CollectMinerals": return WorkerJob.MineralCollector;
            case "Guard": return WorkerJob.Idle;
            default: return WorkerJob.Any;
        }
    }

    void UpdateVision()
    {
        Vector2Int center = GridManager.Instance.WorldToGrid(transform.position);
        if (!GridManager.Instance.IsValid(center.x, center.y)) return;

        int r = Mathf.CeilToInt(visionRadius);
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    GridManager.Instance.RevealFog(center.x + x, center.y + y);
                }
            }
        }
    }

    public void PickNewWanderTarget()
    {
        Vector2 basePos = Queen.Instance != null
            ? (Vector2)Queen.Instance.transform.position
            : (Vector2)transform.position;

        wanderTarget = basePos + Random.insideUnitCircle * 4f;
        wanderTimer = Random.Range(1f, 3f);
    }

    public void FollowPathTo(Vector2 targetPos)
    {
        pathCooldown -= Time.deltaTime;
        if (currentPath == null || pathIndex >= currentPath.Count || pathCooldown <= 0f)
        {
            Vector2? oldWaypoint = null;
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                oldWaypoint = currentPath[pathIndex];
            }

            currentPath = GetPathInternal(transform.position, targetPos);
            pathIndex = 0;

            if (currentPath != null && currentPath.Count > 0)
            {
                int bestIndex = 0;
                bool foundOld = false;

                if (oldWaypoint.HasValue)
                {
                    for (int i = 0; i < currentPath.Count; i++)
                    {
                        if (Vector2.Distance(currentPath[i], oldWaypoint.Value) < 0.01f)
                        {
                            bestIndex = i;
                            foundOld = true;
                            break;
                        }
                    }
                }

                if (!foundOld)
                {
                    float minDist = float.MaxValue;
                    for (int i = 0; i < currentPath.Count; i++)
                    {
                        float d = Vector2.Distance(transform.position, currentPath[i]);
                        if (d < minDist)
                        {
                            minDist = d;
                            bestIndex = i;
                        }
                    }
                    if (minDist < 0.1f && bestIndex < currentPath.Count - 1)
                    {
                        bestIndex++;
                    }
                }
                pathIndex = bestIndex;
            }

            pathCooldown = pathRefreshRate;
        }

        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector2 waypoint = currentPath[pathIndex];
            transform.position = Vector2.MoveTowards(transform.position, waypoint, speed * Time.deltaTime);

            Vector2 dir = (waypoint - (Vector2)transform.position).normalized;
            if (dir != Vector2.zero) transform.right = dir;

            if (Vector2.Distance(transform.position, waypoint) < 0.1f)
            {
                pathIndex++;
            }
        }
    }

    public Vector2 FindNearestWalkableTo(Vector2 worldTarget)
    {
        Vector2Int targetGrid = GridManager.Instance.WorldToGrid(worldTarget);
        if (!GridManager.Instance.IsValid(targetGrid.x, targetGrid.y)) return transform.position;

        if (GridManager.Instance.IsWalkable(targetGrid)) return worldTarget;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(targetGrid);
        visited.Add(targetGrid);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        int maxSearch = 200;
        while (queue.Count > 0 && maxSearch-- > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (GridManager.Instance.IsWalkable(current))
            {
                return GridManager.Instance.GridToWorld(current.x, current.y);
            }

            foreach (Vector2Int d in dirs)
            {
                Vector2Int next = current + d;
                if (GridManager.Instance.IsValid(next.x, next.y) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return transform.position;
    }

    public List<Vector2> GetPathInternal(Vector2 start, Vector2 end)
    {
        if (PathCache.Instance != null && PathCache.Instance.TryGet(start, end, out var cached))
            return cached;

        if (Pathfinder.Instance == null) return null;

        var path = Pathfinder.Instance.FindPath(start, end);

        if (path != null && PathCache.Instance != null)
            PathCache.Instance.Store(start, end, path);

        return path;
    }
}