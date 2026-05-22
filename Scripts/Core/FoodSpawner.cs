using UnityEngine;
using System.Collections.Generic;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner Instance;

    [Header("Spawning Settings")]
    public float spawnInterval = 30f;
    public int maxFoodPiles = 10;
    public int initialFoodCount = 3;
    public float spawnRadius = 8f;
    
    [Header("Prefabs")]
    public GameObject foodPrefab;

    private float spawnTimer = 0f;
    
    public List<Food> activeFoodPiles = new List<Food>();
    
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Cleanup depleted food from list
        activeFoodPiles.RemoveAll(f => f == null || f.isDepleted);

        if (activeFoodPiles.Count >= maxFoodPiles) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnFoodNearColony();
        }
    }

    public void SpawnInitialFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFoodNearColony();
        }
    }

    private void SpawnFoodNearColony()
    {
        if (GridManager.Instance == null || Queen.Instance == null) return;

        Vector2 queenPos = Queen.Instance.transform.position;
        
        // Try up to 10 times to find a valid walkable spot near the queen
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius; // spawn within given radius
            Vector2 attemptPos = queenPos + randomOffset;
            
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(attemptPos);
            
            if (GridManager.Instance.IsValid(gridPos.x, gridPos.y) && GridManager.Instance.IsWalkable(gridPos))
            {
                // Found a valid spot!
                CreateFoodPile(GridManager.Instance.GridToWorld(gridPos.x, gridPos.y));
                return;
            }
        }
    }

    private void CreateFoodPile(Vector2 position)
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("FoodSpawner cannot spawn food: foodPrefab is missing!");
            return;
        }

        GameObject foodObj = Instantiate(foodPrefab, position, Quaternion.identity);
        Food food = foodObj.GetComponent<Food>();
        if (food == null) food = foodObj.AddComponent<Food>();
        
        // Randomize type: 70% berry(3), 20% meat(8), 10% crumb(1)
        int rand = Random.Range(0, 100);
        int amount = 3;
        if (rand > 90) amount = 1;
        else if (rand > 70) amount = 8;
        
        food.Init(amount);
        
        activeFoodPiles.Add(food);
    }

}
