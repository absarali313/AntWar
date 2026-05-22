using UnityEngine;
using System.Collections.Generic;

public enum AntRole
{
    Worker,
    Soldier
}

public enum WorkerJob
{
    Digger,
    Idle,
    Returning,
    FoodCollector
}

public class Ant : MonoBehaviour
{
    public AntRole role = AntRole.Worker;
    public WorkerJob currentJob = WorkerJob.Idle;
    public float speed = 2f;
    public float visionRadius = 3f;

    // Pathfinding
    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathCooldown = 0f;
    private float pathRefreshRate = 1f;

    // Per-ant stagger offset (prevents all ants computing paths on the same frame)
    private int idOffset;

    // Wander
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;

    // Dig timing
    private float digCooldown = 0f;
    private float digRate = 0.4f; // seconds between each dig strike

    // Food collection
    private Food targetFood;
    private int carryingFoodAmount = 0;
    private int carryCapacity = 3;

    void Start()
    {
        idOffset = Random.Range(0, 60);
        currentJob = WorkerJob.Idle;
        PickNewWanderTarget();
    }

    void Update()
    {
        // Vision runs on a staggered schedule (cheap)
        if ((Time.frameCount + idOffset) % 6 == 0)
        {
            UpdateVision();
        }

        // Behavior runs EVERY frame so movement is smooth
        switch (role)
        {
            case AntRole.Worker:
                WorkerBehavior();
                break;
            case AntRole.Soldier:
                SoldierBehavior();
                break;
        }
    }

    void OnDestroy()
    {
        if (DigCommandManager.Instance != null)
        {
            DigCommandManager.ReleaseAnt(this);
        }
    }

    // ───────────────────────────────────────
    // VISION (fog of war reveal)
    // ───────────────────────────────────────
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

    // ───────────────────────────────────────
    // WORKER BEHAVIOR
    // ───────────────────────────────────────
    void WorkerBehavior()
    {
        // 1. Mandatory returning (e.g. from digging)
        if (currentJob == WorkerJob.Returning)
        {
            ReturnBehavior();
            return;
        }

        // 2. If carrying food, MUST deliver it
        if (carryingFoodAmount > 0)
        {
            currentJob = WorkerJob.FoodCollector;
        }
        else
        {
            // 3. Check for Dig commands
            if (DigCommandManager.HasTargetForAnt(this))
            {
                currentJob = WorkerJob.Digger;
            }
            // 4. Check for Food to collect
            else if (targetFood != null && !targetFood.isDepleted)
            {
                currentJob = WorkerJob.FoodCollector;
            }
            else
            {
                // Try to find food, otherwise idle
                FindFood();
                currentJob = (targetFood != null) ? WorkerJob.FoodCollector : WorkerJob.Idle;
            }
        }

        switch (currentJob)
        {
            case WorkerJob.Digger:
                DiggerBehavior();
                break;
            case WorkerJob.FoodCollector:
                FoodCollectorBehavior();
                break;
            case WorkerJob.Idle:
                IdleBehavior();
                break;
        }
    }

    // ───────────────────────────────────────
    // IDLE: Wander around open tunnels
    // ───────────────────────────────────────
    void IdleBehavior()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            PickNewWanderTarget();
        }

        // Simple direct movement toward wander target
        float distToTarget = Vector2.Distance(transform.position, wanderTarget);
        if (distToTarget < 0.3f)
        {
            PickNewWanderTarget();
            return;
        }

        Vector2 dir = (wanderTarget - (Vector2)transform.position).normalized;
        Vector2 nextPos = (Vector2)transform.position + dir * speed * Time.deltaTime;

        Vector2Int nextPosGrid = GridManager.Instance.WorldToGrid(nextPos);
        Vector2Int currentGrid = GridManager.Instance.WorldToGrid(transform.position);

        if (!GridManager.Instance.IsWalkable(currentGrid))
        {
            // Fail-safe: If we somehow clipped into a wall, pop out to the nearest open space immediately
            transform.position = FindNearestWalkableTo(transform.position);
            PickNewWanderTarget();
            return;
        }

        if (GridManager.Instance.IsValid(nextPosGrid.x, nextPosGrid.y) && GridManager.Instance.IsWalkable(nextPosGrid))
        {
            transform.position = nextPos;
            if (dir != Vector2.zero) transform.right = dir;
        }
        else
        {
            // Hit a wall normally
            PickNewWanderTarget();
        }
    }

    void PickNewWanderTarget()
    {
        // Pick a random open tile near the queen
        Vector2 basePos = Queen.Instance != null
            ? (Vector2)Queen.Instance.transform.position
            : (Vector2)transform.position;

        wanderTarget = basePos + Random.insideUnitCircle * 4f;
        wanderTimer = Random.Range(1f, 3f);
    }

    // ───────────────────────────────────────
    // DIGGER: AoE2-style — walk to target, dig through walls
    // ───────────────────────────────────────
    void DiggerBehavior()
    {
        // Each ant picks the nearest target from the queue or keeps its assigned one
        Vector2? maybeTarget = DigCommandManager.GetTargetForAnt(this);
        if (maybeTarget == null) 
        {
            currentJob = WorkerJob.Idle;
            return;
        }

        Vector2 target = maybeTarget.Value;

        // Check if target tile is already dug and we're close enough
        Vector2Int targetGrid = GridManager.Instance.WorldToGrid(target);
        if (GridManager.Instance.IsValid(targetGrid.x, targetGrid.y) && GridManager.Instance.IsWalkable(targetGrid))
        {
            float dist = Vector2.Distance(transform.position, target);
            if (dist < 0.5f)
            {
                // This specific target is done — remove it and head home
                DigCommandManager.RemoveTarget(target);
                currentPath = null;
                currentJob = WorkerJob.Returning;
                return;
            }
        }

        // Step 1: Check if we are already at the frontier
        Vector2 frontier = FindNearestWalkableTo(target);
        if (Vector2.Distance(transform.position, frontier) < 0.5f)
        {
            // We are at the frontier, start digging!
            currentPath = null;
            DigTowardTarget(target);
            return;
        }

        // Step 2: Compute path to frontier
        pathCooldown -= Time.deltaTime;
        bool needsNewPath = (currentPath == null || pathIndex >= currentPath.Count || pathCooldown <= 0f);

        if (needsNewPath)
        {
            // Only recompute on stagger to spread CPU load, BUT if we have no path at all, do it immediately
            bool canRecompute = (currentPath == null) || ((Time.frameCount + idOffset) % 8 == 0);

            if (canRecompute)
            {
                Vector2? oldWaypoint = null;
                if (currentPath != null && pathIndex < currentPath.Count)
                {
                    oldWaypoint = currentPath[pathIndex];
                }

                currentPath = GetPath(transform.position, frontier);
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
        }

        // Step 3: Follow current path
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

    // BFS outward from the target to find the nearest walkable tile
    Vector2 FindNearestWalkableTo(Vector2 worldTarget)
    {
        Vector2Int targetGrid = GridManager.Instance.WorldToGrid(worldTarget);
        if (!GridManager.Instance.IsValid(targetGrid.x, targetGrid.y)) return transform.position;

        // If target itself is walkable, just return it
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

        // Fallback: stay where we are
        return transform.position;
    }

    void DigTowardTarget(Vector2 target)
    {
        // Find which adjacent tile is closest to the target and blocked — dig it
        Vector2Int myGrid = GridManager.Instance.WorldToGrid(transform.position);
        if (!GridManager.Instance.IsValid(myGrid.x, myGrid.y)) return;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        Vector2Int bestGrid = new Vector2Int(-1, -1);
        float bestDist = float.MaxValue;
        Vector2Int bestDir = Vector2Int.zero;

        foreach (Vector2Int d in dirs)
        {
            Vector2Int neighbor = myGrid + d;
            if (GridManager.Instance.IsValid(neighbor.x, neighbor.y) && !GridManager.Instance.IsWalkable(neighbor))
            {
                Vector2 neighborWorld = GridManager.Instance.GridToWorld(neighbor.x, neighbor.y);
                float dist = Vector2.Distance(neighborWorld, target);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestGrid = neighbor;
                    bestDir = d;
                }
            }
        }

        if (bestGrid.x != -1)
        {
            // Dig cooldown — ants swing, pause, swing again
            digCooldown -= Time.deltaTime;
            transform.right = new Vector2(bestDir.x, bestDir.y); // Face the wall while waiting

            if (digCooldown <= 0f)
            {
                GridManager.Instance.DamageTile(bestGrid.x, bestGrid.y, 1);
                digCooldown = digRate;

                // If the tile just broke, invalidate path so we recompute next frame
                if (GridManager.Instance.IsWalkable(bestGrid))
                {
                    currentPath = null;
                    pathCooldown = 0f;

                    // Clear the path cache since the grid changed
                    if (PathCache.Instance != null)
                        PathCache.Instance.Clear();
                }
            }
        }
    }

    // ───────────────────────────────────────
    // FOOD COLLECTOR BEHAVIOR
    // ───────────────────────────────────────
    void FoodCollectorBehavior()
    {
        // State A: Deliver Food
        if (carryingFoodAmount > 0)
        {
            DeliverFood();
            return;
        }

        // State B: Find Food
        if (targetFood == null || targetFood.isDepleted)
        {
            FindFood();
        }

        // State C: Move to Food & Collect
        if (targetFood != null && !targetFood.isDepleted)
        {
            float dist = Vector2.Distance(transform.position, targetFood.transform.position);
            if (dist < 0.5f)
            {
                // Reached food, take it
                CollectFood();
            }
            else
            {
                // Move towards it
                FollowPathTo(targetFood.transform.position);
            }
        }
        else
        {
            // No food found, fall back to idle
            currentJob = WorkerJob.Idle;
        }
    }

    void FindFood()
    {
        targetFood = null;
        if (FoodSpawner.Instance == null) return;

        float bestDist = float.MaxValue;
        float searchRadius = 15f;

        foreach (Food f in FoodSpawner.Instance.activeFoodPiles)
        {
            if (f != null && !f.isDepleted)
            {
                float d = Vector2.Distance(transform.position, f.transform.position);
                if (d <= searchRadius && d < bestDist)
                {
                    // Check if path is actually possible (prevent getting stuck trying to reach unreachable food)
                    Vector2Int gridPos = GridManager.Instance.WorldToGrid(f.transform.position);
                    if (GridManager.Instance.IsWalkable(gridPos))
                    {
                        bestDist = d;
                        targetFood = f;
                    }
                }
            }
        }
    }

    void CollectFood()
    {
        int taken = targetFood.TakeFood(carryCapacity);
        carryingFoodAmount += taken;
        currentPath = null; // Reset path so we pathfind home
    }

    void DeliverFood()
    {
        if (FoodStorage.Instance == null)
        {
            // Nowhere to put it, just idle (or walk to queen)
            currentJob = WorkerJob.Idle;
            return;
        }

        Vector2 storagePos = FoodStorage.Instance.transform.position;
        float dist = Vector2.Distance(transform.position, storagePos);

        if (dist < 1.0f)
        {
            FoodStorage.Instance.Deposit(carryingFoodAmount);
            carryingFoodAmount = 0;
            targetFood = null;
            currentPath = null;
        }
        else
        {
            FollowPathTo(storagePos);
        }
    }

    // Extracted path following logic for reuse
    void FollowPathTo(Vector2 targetPos)
    {
        pathCooldown -= Time.deltaTime;
        if (currentPath == null || pathIndex >= currentPath.Count || pathCooldown <= 0f)
        {
            Vector2? oldWaypoint = null;
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                oldWaypoint = currentPath[pathIndex];
            }

            currentPath = GetPath(transform.position, targetPos);
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

    // ───────────────────────────────────────
    // RETURNING: Walk back to queen after completing a dig
    // ───────────────────────────────────────
    void ReturnBehavior()
    {
        if (Queen.Instance == null)
        {
            currentJob = WorkerJob.Idle;
            return;
        }

        Vector2 queenPos = Queen.Instance.transform.position;
        float dist = Vector2.Distance(transform.position, queenPos);

        // Close enough to queen — switch to idle
        if (dist < 2f)
        {
            currentJob = WorkerJob.Idle;
            currentPath = null;
            return;
        }

        // If new dig commands appeared while returning, switch to digging
        if (DigCommandManager.HasTargets)
        {
            currentJob = WorkerJob.Digger;
            currentPath = null;
            return;
        }

        // Walk home via pathfinding
        pathCooldown -= Time.deltaTime;
        if (currentPath == null || pathIndex >= currentPath.Count || pathCooldown <= 0f)
        {
            Vector2? oldWaypoint = null;
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                oldWaypoint = currentPath[pathIndex];
            }

            currentPath = GetPath(transform.position, queenPos);
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
                pathIndex++;
        }
    }

    // ───────────────────────────────────────
    // SOLDIER: Guard the queen
    // ───────────────────────────────────────
    void SoldierBehavior()
    {
        if (Queen.Instance == null) return;

        Vector2 queenPos = Queen.Instance.transform.position;
        float dist = Vector2.Distance(transform.position, queenPos);

        if (dist > 2f)
        {
            Vector2 dir = (queenPos - (Vector2)transform.position).normalized;
            Vector2 nextPos = (Vector2)transform.position + dir * speed * Time.deltaTime;

            Vector2Int nextPosGrid = GridManager.Instance.WorldToGrid(nextPos);
            Vector2Int currentGrid = GridManager.Instance.WorldToGrid(transform.position);

            if (!GridManager.Instance.IsWalkable(currentGrid))
            {
                transform.position = FindNearestWalkableTo(transform.position);
                return;
            }

            if (GridManager.Instance.IsValid(nextPosGrid.x, nextPosGrid.y) && GridManager.Instance.IsWalkable(nextPosGrid))
            {
                transform.position = nextPos;
                if (dir != Vector2.zero) transform.right = dir;
            }
        }
    }

    // ───────────────────────────────────────
    // PATHFINDING HELPERS
    // ───────────────────────────────────────
    List<Vector2> GetPath(Vector2 start, Vector2 end)
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