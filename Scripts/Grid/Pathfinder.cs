using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    void Awake()
    {
        Instance = this;
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 target)
    {
        Tile startTile = GridManager.Instance.GetTileFromWorld(start);
        Tile targetTile = GridManager.Instance.GetTileFromWorld(target);

        if (startTile == null || targetTile == null) return null;

        Vector2Int startNode = startTile.gridPosition;
        Vector2Int targetNode = targetTile.gridPosition;

        // Early exit if already at destination
        if (startNode == targetNode) return new List<Vector2> { target };

        List<Vector2Int> open = new List<Vector2Int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, PathNode> nodes = new Dictionary<Vector2Int, PathNode>();

        open.Add(startNode);
        nodes[startNode] = new PathNode
        {
            pos = startNode,
            gCost = 0,
            hCost = Heuristic(startNode, targetNode)
        };

        int maxIterations = 50000; // Safety cap to prevent freezes on large grids
        int iterations = 0;

        while (open.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            Vector2Int current = GetLowestF(open, nodes);
            open.Remove(current);
            closed.Add(current);

            if (current == targetNode)
                return Reconstruct(nodes[current]);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closed.Contains(neighbor)) continue;
                if (!GridManager.Instance.IsWalkable(neighbor)) continue;

                int tentativeG = nodes[current].gCost + 1;

                if (!nodes.ContainsKey(neighbor) || tentativeG < nodes[neighbor].gCost)
                {
                    if (!nodes.ContainsKey(neighbor))
                    {
                        nodes[neighbor] = new PathNode { pos = neighbor };
                        open.Add(neighbor);
                    }

                    nodes[neighbor].gCost = tentativeG;
                    nodes[neighbor].hCost = Heuristic(neighbor, targetNode);
                    nodes[neighbor].parent = nodes[current];
                }
            }
        }

        return null; // No path found
    }

    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 2;
    }

    List<Vector2> Reconstruct(PathNode node)
    {
        List<Vector2> path = new List<Vector2>();

        while (node != null)
        {
            Tile tile = GridManager.Instance.GetTile(node.pos.x, node.pos.y);
            if (tile != null)
            {
                path.Add(tile.transform.position);
            }
            node = node.parent;
        }

        path.Reverse();
        return path;
    }

    Vector2Int GetLowestF(List<Vector2Int> open, Dictionary<Vector2Int, PathNode> nodes)
    {
        Vector2Int best = open[0];
        int bestF = nodes[best].fCost;

        for (int i = 1; i < open.Count; i++)
        {
            int f = nodes[open[i]].fCost;
            if (f < bestF)
            {
                best = open[i];
                bestF = f;
            }
        }

        return best;
    }

    List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };
    }
}
