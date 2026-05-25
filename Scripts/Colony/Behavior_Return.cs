using System.Collections.Generic;
using UnityEngine;

public class Behavior_Return : IAntBehavior
{
    public string BehaviorName => "Returning";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        if (Queen.Instance == null) return true;
        float dist = Vector2.Distance(ant.transform.position, Queen.Instance.transform.position);
        return dist < 2f;
    }

    public void OnAssigned(Ant ant)
    {
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        ReturnBehavior(ant);
    }

    public bool IsComplete(Ant ant)
    {
        if (Queen.Instance == null) return true;
        float dist = Vector2.Distance(ant.transform.position, Queen.Instance.transform.position);
        return dist < 2f;
    }

    private void ReturnBehavior(Ant ant)
    {
        if (Queen.Instance == null)
        {
            return;
        }

        Vector2 queenPos = Queen.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, queenPos);

        if (dist < 2f)
        {
            return;
        }

        if (DigCommandManager.HasTargets)
        {
            ant.wantsToReturn = false;
        }

        ant.pathCooldown -= Time.deltaTime;
        if (ant.currentPath == null || ant.pathIndex >= ant.currentPath.Count || ant.pathCooldown <= 0f)
        {
            Vector2? oldWaypoint = null;
            if (ant.currentPath != null && ant.pathIndex < ant.currentPath.Count)
            {
                oldWaypoint = ant.currentPath[ant.pathIndex];
            }

            ant.currentPath = ant.GetPathInternal(ant.transform.position, queenPos);
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

        if (ant.currentPath != null && ant.pathIndex < ant.currentPath.Count)
        {
            Vector2 waypoint = ant.currentPath[ant.pathIndex];
            ant.transform.position = Vector2.MoveTowards(ant.transform.position, waypoint, ant.speed * Time.deltaTime);

            Vector2 dir = (waypoint - (Vector2)ant.transform.position).normalized;
            if (dir != Vector2.zero) ant.transform.right = dir;

            if (Vector2.Distance(ant.transform.position, waypoint) < 0.1f)
                ant.pathIndex++;
        }
    }
}