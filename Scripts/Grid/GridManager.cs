using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public int width = 20;
    public int height = 20;
    public float tileSize = 1f;

    public GameObject tilePrefab;

    private Dictionary<Vector2Int, Tile> grid = new Dictionary<Vector2Int, Tile>();

    void Start()
    {
        GenerateGrid();
    }

    // Pheromone decay disabled — re-enable when pheromone gameplay is implemented

    void GenerateGrid()
    {

       Vector3 offset = new Vector3(
            -(width * tileSize) / 2f + tileSize / 2f,
            -(height * tileSize) / 2f + tileSize / 2f,
            0
        );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, y * tileSize, 0) + offset;

                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(x, y);

                grid[new Vector2Int(x, y)] = tile;
            }
        }
    }

    public bool IsBlocked(Vector2 worldPos)
    {
        Tile tile = GetTileFromWorld(worldPos);

        if (tile == null)
            return true;

        return tile.blocked;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        Tile tile = GetTile(pos.x, pos.y);
        return tile != null && !tile.blocked;
    }
    
    public Tile GetTileFromWorld(Vector2 worldPos)
    {
        float x = worldPos.x + (width * tileSize) / 2f;
        float y = worldPos.y + (height * tileSize) / 2f;

        int gx = Mathf.FloorToInt(x / tileSize);
        int gy = Mathf.FloorToInt(y / tileSize);

        Vector2Int pos = new Vector2Int(gx, gy);

        if (grid.ContainsKey(pos))
            return grid[pos];

        return null;
    }

    public Tile GetTile(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (grid.ContainsKey(pos))
            return grid[pos];

        return null;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        float x = worldPos.x + (width * tileSize) / 2f;
        float y = worldPos.y + (height * tileSize) / 2f;

        int gx = Mathf.FloorToInt(x / tileSize);
        int gy = Mathf.FloorToInt(y / tileSize);

        return new Vector2Int(gx, gy);
    }

    public Vector2 GridToWorld(int x, int y)
    {
        Vector3 offset = new Vector3(
            -(width * tileSize) / 2f + tileSize / 2f,
            -(height * tileSize) / 2f + tileSize / 2f,
            0
        );
        return new Vector2(x * tileSize, y * tileSize) + (Vector2)offset;
    }

    public bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void DamageTile(int x, int y, int amount)
    {
        Tile tile = GetTile(x, y);
        if (tile != null)
        {
            tile.Damage(amount);
        }
    }

    public void RevealFog(int x, int y)
    {
        Tile tile = GetTile(x, y);
        if (tile != null)
        {
            tile.Reveal();
            tile.UpdateFogVisual();
        }
    }
}