using UnityEngine;

public class StarterHiveBuilder : MonoBehaviour
{
    // We automatically find the Grid, so you only need to assign these prefabs!
    public GameObject antPrefab;
    public GameObject queenPrefab;
    
    [Header("Economy Prefabs")]
    public GameObject foodStoragePrefab;

    public int chamberSize = 3;
    public int tunnelLength = 6;
    public int workerCount = 3;

    void Start()
    {
        // Wait just a tiny bit so the GridManager has time to generate the dirt first!
        Invoke("InitializeHive", 0.1f);
    }

    void InitializeHive()
    {
        BuildHive();
        RevealInitialArea();
        PlaceQueen();
        SpawnAnts();
        SetupEconomy();
    }

    void BuildHive()
    {
        CreateQueenChamber();
        CreateTunnels();
    }

    void CreateQueenChamber()
    {
        int cx = GridManager.Instance.width / 2;
        int cy = GridManager.Instance.height / 2;

        for (int x = -chamberSize; x <= chamberSize; x++)
        {
            for (int y = -chamberSize; y <= chamberSize; y++)
            {
                // Carve a protective circle instead of a square
                if (x * x + y * y <= chamberSize * chamberSize)
                {
                    Tile tile = GridManager.Instance.GetTile(cx + x, cy + y);
                    if (tile != null)
                    {
                        tile.Dig();
                    }
                }
            }
        }
    }

    void RevealInitialArea()
    {
        int cx = GridManager.Instance.width / 2;
        int cy = GridManager.Instance.height / 2;

        // 20x20 area means -10 to +10 from the center
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                Tile tile = GridManager.Instance.GetTile(cx + x, cy + y);
                if (tile != null)
                {
                    tile.Reveal();
                    tile.UpdateFogVisual();
                }
            }
        }
    }

    void CreateTunnels()
    {
        int cx = GridManager.Instance.width / 2;
        int cy = GridManager.Instance.height / 2;

        int chokePointLength = chamberSize + 2;

        // Dig a narrow 1-tile wide exit (choke point) going straight down
        for (int i = 0; i <= chokePointLength; i++)
        {
            Tile tile = GridManager.Instance.GetTile(cx, cy - i);
            if (tile != null) tile.Dig();
        }

        int junctionY = cy - chokePointLength;

        // Branch out into the 3 wide tunnels from the end of the choke point
        CreateTunnel(cx, junctionY, Vector2Int.right);
        CreateTunnel(cx, junctionY, Vector2Int.left);
        CreateTunnel(cx, junctionY, Vector2Int.down);
    }

    void CreateTunnel(int startX, int startY, Vector2Int dir)
    {
        int x = startX;
        int y = startY;

        for (int i = 0; i < tunnelLength; i++)
        {
            x += dir.x;
            y += dir.y;

            for (int w = -1; w <= 1; w++)
            {
                for (int h = -1; h <= 1; h++)
                {
                    Tile tile = GridManager.Instance.GetTile(x + w, y + h);
                    if (tile != null)
                    {
                        tile.Dig();
                    }
                }
            }
        }
    }

    void PlaceQueen()
    {
        if (Queen.Instance == null && queenPrefab != null)
        {
            Instantiate(queenPrefab, new Vector3(0.65f, 4.35f, 0), Quaternion.identity);
        }
        else if (Queen.Instance != null)
        {
            Queen.Instance.transform.position = new Vector3(0, 0, 0);
        }
    }

    void SpawnAnts()
    {
        for (int i = 0; i < workerCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 1.5f;
            Vector3 spawnPos = Vector3.zero;
            Instantiate(antPrefab, spawnPos + (Vector3)offset, Quaternion.identity);
        }
    }

    void SetupEconomy()
    {
        // 1. Create Food Storage near Queen
        if (foodStoragePrefab != null)
        {
            Instantiate(foodStoragePrefab, new Vector3(-3.163f, 0.73f, 0f), Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("FoodStorage cannot be spawned: foodStoragePrefab is missing!");
        }

        // 2. Start Food Spawner (Now expected to be placed manually in the scene, e.g., on GameManager)
        if (FoodSpawner.Instance != null)
        {
            FoodSpawner.Instance.SpawnInitialFood();
        }
        else
        {
            Debug.LogWarning("FoodSpawner instance not found! Please attach the FoodSpawner script to your GameManager.");
        }
    }
}
