using UnityEngine;
using System.Collections.Generic;

public class ResourceSpawner : MonoBehaviour
{
    public static ResourceSpawner Instance;

    [Header("Resource Prefabs")]
    public GameObject foodPrefab;
    public GameObject waterPrefab;
    public GameObject mineralPrefab;

    [Header("Initial Spawns")]
    public int initialFood = 2;
    public int initialWater = 1;
    public int initialMinerals = 1;

    [Header("Spawn Rates")]
    public float foodSpawnInterval = 45f;
    public float waterSpawnInterval = 90f;
    public float mineralSpawnInterval = 120f;

    private float foodTimer = 0f;
    private float waterTimer = 0f;
    private float mineralTimer = 0f;

    [Header("Max Resources")]
    public int maxFood = 6;
    public int maxWater = 3;
    public int maxMinerals = 3;

    public List<Food> activeFood = new List<Food>();
    public List<WaterSource> activeWater = new List<WaterSource>();
    public List<MineralDeposit> activeMinerals = new List<MineralDeposit>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < initialFood; i++) SpawnFood();
        for (int i = 0; i < initialWater; i++) SpawnWater();
        for (int i = 0; i < initialMinerals; i++) SpawnMineral();
    }

    void Update()
    {
        activeFood.RemoveAll(f => f == null || f.isDepleted);
        activeWater.RemoveAll(w => w == null || w.isDepleted);
        activeMinerals.RemoveAll(m => m == null || m.isDepleted);

        // Food respawns gradually
        if (activeFood.Count < maxFood)
        {
            foodTimer += Time.deltaTime;
            if (foodTimer >= foodSpawnInterval)
            {
                foodTimer = 0f;
                SpawnFood();
            }
        }

        // Water spawns slower
        if (activeWater.Count < maxWater)
        {
            waterTimer += Time.deltaTime;
            if (waterTimer >= waterSpawnInterval)
            {
                waterTimer = 0f;
                SpawnWater();
            }
        }

        // Minerals spawn slowest
        if (activeMinerals.Count < maxMinerals)
        {
            mineralTimer += Time.deltaTime;
            if (mineralTimer >= mineralSpawnInterval)
            {
                mineralTimer = 0f;
                SpawnMineral();
            }
        }
    }

    Vector2Int? FindSpawnPosition(float minDistanceFromQueen = 3f)
    {
        if (GridManager.Instance == null) return null;
        
        var walkable = GridManager.Instance.walkableTiles;
        if (walkable.Count == 0) return null;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2Int pos = walkable[Random.Range(0, walkable.Count)];
            
            if (Queen.Instance != null)
            {
                float dist = Vector2.Distance(
                    GridManager.Instance.GridToWorld(pos.x, pos.y),
                    Queen.Instance.transform.position
                );
                if (dist < minDistanceFromQueen) continue;
            }
            
            return pos;
        }
        
        return null;
    }

    void SpawnFood()
    {
        var pos = FindSpawnPosition(2f);
        if (pos == null || foodPrefab == null) return;
        
        Vector2 worldPos = GridManager.Instance.GridToWorld(pos.Value.x, pos.Value.y);
        GameObject obj = Instantiate(foodPrefab, worldPos, Quaternion.identity);
        Food food = obj.GetComponent<Food>();
        if (food == null) food = obj.AddComponent<Food>();
        food.Init();
        activeFood.Add(food);
    }

    void SpawnWater()
    {
        var pos = FindSpawnPosition(4f);
        if (pos == null || waterPrefab == null) return;
        
        Vector2 worldPos = GridManager.Instance.GridToWorld(pos.Value.x, pos.Value.y);
        GameObject obj = Instantiate(waterPrefab, worldPos, Quaternion.identity);
        WaterSource water = obj.GetComponent<WaterSource>();
        if (water == null) water = obj.AddComponent<WaterSource>();
        water.Init();
        activeWater.Add(water);
    }

    void SpawnMineral()
    {
        var pos = FindSpawnPosition(5f);
        if (pos == null || mineralPrefab == null) return;
        
        Vector2 worldPos = GridManager.Instance.GridToWorld(pos.Value.x, pos.Value.y);
        GameObject obj = Instantiate(mineralPrefab, worldPos, Quaternion.identity);
        MineralDeposit mineral = obj.GetComponent<MineralDeposit>();
        if (mineral == null) mineral = obj.AddComponent<MineralDeposit>();
        mineral.Init();
        activeMinerals.Add(mineral);
    }
}