using System.Collections.Generic;
using UnityEngine;

public class ExplorationMap : MonoBehaviour
{
    public static ExplorationMap Instance;

    void Awake()
    {
        Instance = this;
    }

    private HashSet<Vector2Int> explored = new HashSet<Vector2Int>();

    public void MarkExplored(Vector2Int pos)
    {
        explored.Add(pos);
    }

    public bool IsExplored(Vector2Int pos)
    {
        return explored.Contains(pos);
    }
}
