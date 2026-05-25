using UnityEngine;
using System.Collections.Generic;

// Holds dig targets with 1-ant-per-target assignment.
// Left-click = add target, Right-click = cancel all.
public class DigCommandManager : MonoBehaviour
{
    public static DigCommandManager Instance;

    // Each target and which ant (if any) is assigned to it
    private static List<DigTarget> targets = new List<DigTarget>();

    public static bool HasTargets => targets.Count > 0;

    public int targetCount => targets.Count;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Left-click: add a new dig target
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            AddTarget(worldPos);
        }

        // Right-click: cancel all dig targets
        if (Input.GetMouseButtonDown(1))
        {
            ClearAllTargets();
        }
    }

    public static void AddTarget(Vector2 worldPos)
    {
        // Avoid duplicate targets on the same tile
        foreach (var existing in targets)
        {
            if (Vector2.Distance(existing.position, worldPos) < 0.5f)
                return;
        }
        targets.Add(new DigTarget { position = worldPos, assignedAnt = null });
    }

    // Get the nearest UNASSIGNED target, or the target already assigned to this ant
    public static Vector2? GetTargetForAnt(Ant ant)
    {
        // First check if this ant already has an assignment
        foreach (var t in targets)
        {
            if (t.assignedAnt == ant)
                return t.position;
        }

        // Find nearest unassigned target
        float bestDist = float.MaxValue;
        DigTarget bestTarget = null;

        foreach (var t in targets)
        {
            if (t.assignedAnt != null) continue; // Already taken

            float dist = Vector2.Distance(ant.transform.position, t.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = t;
            }
        }

        if (bestTarget != null)
        {
            bestTarget.assignedAnt = ant;
            return bestTarget.position;
        }

        return null; // All targets are assigned to other ants
    }

    // Check if there are any unassigned targets or targets assigned to this ant
    public static bool HasTargetForAnt(Ant ant)
    {
        foreach (var t in targets)
        {
            if (t.assignedAnt == ant || t.assignedAnt == null)
                return true;
        }
        return false;
    }

    // Remove a specific target (when ant finishes digging it)
    public static void RemoveTarget(Vector2 worldPos)
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (Vector2.Distance(targets[i].position, worldPos) < 0.5f)
            {
                targets.RemoveAt(i);
                return;
            }
        }
    }

    // Release assignment when an ant dies or gets reassigned
    public static void ReleaseAnt(Ant ant)
    {
        foreach (var t in targets)
        {
            if (t.assignedAnt == ant)
            {
                t.assignedAnt = null;
            }
        }
    }

    public static void ClearAllTargets()
    {
        targets.Clear();
    }

    // Inner class to hold target + assignment
    private class DigTarget
    {
        public Vector2 position;
        public Ant assignedAnt;
    }
}
