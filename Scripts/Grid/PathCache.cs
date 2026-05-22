using System.Collections.Generic;
using UnityEngine;

public class PathCache : MonoBehaviour
{
    public static PathCache Instance;

    void Awake()
    {
        Instance = this;
    }

    // Cache keyed by grid coordinates (start, end)
    private Dictionary<(Vector2Int, Vector2Int), List<Vector2>> cache = new Dictionary<(Vector2Int, Vector2Int), List<Vector2>>();
    private Queue<(Vector2Int, Vector2Int)> order = new Queue<(Vector2Int, Vector2Int)>();
    private const int MaxCacheSize = 500; // Prevent uncontrolled growth

    public bool TryGet(Vector2 start, Vector2 end, out List<Vector2> path)
    {
        var key = GetKey(start, end);
        if (cache.TryGetValue(key, out path))
            return true;

        // Check reverse direction – path can be reused reversed
        var revKey = GetKey(end, start);
        if (cache.TryGetValue(revKey, out var revPath))
        {
            // Return a copy reversed to avoid mutating cached list
            path = new List<Vector2>(revPath);
            path.Reverse();
            return true;
        }

        path = null;
        return false;
    }

    public void Store(Vector2 start, Vector2 end, List<Vector2> path)
    {
        var key = GetKey(start, end);
        cache[key] = path;
        order.Enqueue(key);
        // Evict oldest if we exceed capacity
        while (order.Count > MaxCacheSize)
        {
            var oldKey = order.Dequeue();
            cache.Remove(oldKey);
        }
    }

    private (Vector2Int, Vector2Int) GetKey(Vector2 a, Vector2 b)
    {
        Tile startTile = GridManager.Instance.GetTileFromWorld(a);
        Tile endTile = GridManager.Instance.GetTileFromWorld(b);
        if (startTile == null || endTile == null)
            return (new Vector2Int(-1, -1), new Vector2Int(-1, -1)); // sentinel for invalid
        return (startTile.gridPosition, endTile.gridPosition);
    }

    public void Clear()
    {
        cache.Clear();
        order.Clear();
    }
}
