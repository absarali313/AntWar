using UnityEngine;
using System.Collections.Generic;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner Instance;

    [Header("Spawning Settings")]
    public float spawnInterval = 30f;
    public int maxFoodPiles = 10;
    public int initialFoodCount = 3;
    
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
        if (GridManager.Instance == null || GridManager.Instance.walkableTiles.Count == 0) return;
        
        // Try up to 10 times to find a random walkable spot
        for (int i = 0; i < 10; i++)
        {
            Vector2Int randomGridPos = GridManager.Instance.walkableTiles[Random.Range(0, GridManager.Instance.walkableTiles.Count)];
            Vector2 attemptPos = GridManager.Instance.GridToWorld(randomGridPos.x, randomGridPos.y);
            
            // Check if there's already food here to avoid stacking perfectly on top of each other
            bool spotTaken = false;
            foreach (Food f in activeFoodPiles)
            {
                if (f != null && Vector2.Distance(f.transform.position, attemptPos) < 0.1f)
                {
                    spotTaken = true;
                    break;
                }
            }

            if (!spotTaken)
            {
                CreateFoodPile(attemptPos);
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
        
        // Initialize using the prefab's preset amount
        food.Init();
        
        activeFoodPiles.Add(food);
    }

}
