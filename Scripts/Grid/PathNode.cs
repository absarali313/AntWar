using UnityEngine;

public class PathNode
{
    public Vector2Int pos;
    public bool walkable;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public PathNode parent;
}
