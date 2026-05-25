using System.Collections.Generic;
using UnityEngine;

public class Behavior_Dig : IAntBehavior
{
    public string BehaviorName => "Dig";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        Vector2? maybeTarget = DigCommandManager.GetTargetForAnt(ant);
        if (maybeTarget == null) return true;
        Vector2 target = maybeTarget.Value;
        Vector2 frontier = ant.FindNearestWalkableTo(target);
        float dist = Vector2.Distance(ant.transform.position, frontier);
        return dist > 0.5f;
    }

    public void OnAssigned(Ant ant)
    {
        DigCommandManager.ReleaseAnt(ant);
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        DiggerBehavior(ant);
    }

    public bool IsComplete(Ant ant)
    {
        return DigCommandManager.GetTargetForAnt(ant) == null;
    }

    private void DiggerBehavior(Ant ant)
    {
        Vector2? maybeTarget = DigCommandManager.GetTargetForAnt(ant);
        if (maybeTarget == null) return;

        Vector2 target = maybeTarget.Value;
        Vector2Int targetGrid = GridManager.Instance.WorldToGrid(target);
        if (GridManager.Instance.IsValid(targetGrid.x, targetGrid.y) && GridManager.Instance.IsWalkable(targetGrid))
        {
            float dist = Vector2.Distance(ant.transform.position, target);
            if (dist < 0.5f)
            {
                DigCommandManager.RemoveTarget(target);
                ant.currentPath = null;
                ant.wantsToReturn = true;
                return;
            }
        }

        Vector2 frontier = ant.FindNearestWalkableTo(target);
        if (Vector2.Distance(ant.transform.position, frontier) < 0.5f)
        {
            ant.currentPath = null;
            DigTowardTarget(ant, target);
            return;
        }

        ant.pathCooldown -= Time.deltaTime;
        bool needsNewPath = (ant.currentPath == null || ant.pathIndex >= ant.currentPath.Count || ant.pathCooldown <= 0f);

        if (needsNewPath)
        {
            bool canRecompute = (ant.currentPath == null) || ((Time.frameCount + ant.idOffset) % 8 == 0);
            if (canRecompute)
            {
                Vector2? oldWaypoint = null;
                if (ant.currentPath != null && ant.pathIndex < ant.currentPath.Count)
                {
                    oldWaypoint = ant.currentPath[ant.pathIndex];
                }

                ant.currentPath = ant.GetPathInternal(ant.transform.position, frontier);
                ant.pathIndex = 0;

                if (ant.currentPath != null && ant.currentPath.Count > 0)
                {
                    int bestIndex = 0;
                    bool foundOld = false;

                    if (oldWaypoint.HasValue)
                    {
                        for (int i = 0; i < ant.currentPath.Count; i++)
                        {
                            if (Vector2.Distance(ant.currentPath[i], oldWaypoint.Value) < 0.01f)
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
                        for (int i = 0; i < ant.currentPath.Count; i++)
                        {
                            float d = Vector2.Distance(ant.transform.position, ant.currentPath[i]);
                            if (d < minDist)
                            {
                                minDist = d;
                                bestIndex = i;
                            }
                        }
                        if (minDist < 0.1f && bestIndex < ant.currentPath.Count - 1)
                        {
                            bestIndex++;
                        }
                    }
                    ant.pathIndex = bestIndex;
                }

                ant.pathCooldown = ant.pathRefreshRate;
            }
        }

        if (ant.currentPath != null && ant.pathIndex < ant.currentPath.Count)
        {
            Vector2 waypoint = ant.currentPath[ant.pathIndex];
            ant.transform.position = Vector2.MoveTowards(ant.transform.position, waypoint, ant.speed * Time.deltaTime);

            Vector2 dir = (waypoint - (Vector2)ant.transform.position).normalized;
            if (dir != Vector2.zero) ant.transform.right = dir;

            if (Vector2.Distance(ant.transform.position, waypoint) < 0.1f)
            {
                ant.pathIndex++;
            }
        }
    }

    private void DigTowardTarget(Ant ant, Vector2 target)
    {
        Vector2Int myGrid = GridManager.Instance.WorldToGrid(ant.transform.position);
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
            ant.digCooldown -= Time.deltaTime;
            ant.transform.right = new Vector2(bestDir.x, bestDir.y);

            if (ant.digCooldown <= 0f)
            {
                GridManager.Instance.DamageTile(bestGrid.x, bestGrid.y, 1);
                ant.digCooldown = ant.digRate;

                if (GridManager.Instance.IsWalkable(bestGrid))
                {
                    ant.currentPath = null;
                    ant.pathCooldown = 0f;

                    if (PathCache.Instance != null)
                        PathCache.Instance.Clear();
                }
            }
        }
    }
}