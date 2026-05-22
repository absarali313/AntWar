using UnityEngine;
using System.Collections.Generic;

public class FoodSpawner : MonoBehaviour
{
    public static FoodSpawner Instance;

    public float spawnInterval = 30f;
    private float spawnTimer = 0f;
    public int maxFoodPiles = 10;
    
    private List<Food> activeFoodPiles = new List<Food>();
    
    // For runtime generation since we don't have a prefab yet
    private Sprite circleSprite;

    void Awake()
    {
        Instance = this;
        CreateCircleSprite();
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
        for (int i = 0; i < 3; i++)
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
            Vector2 randomOffset = Random.insideUnitCircle * 8f; // spawn within 8 units
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
        GameObject foodObj = new GameObject("FoodPile");
        foodObj.transform.position = position;
        
        SpriteRenderer sr = foodObj.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.color = new Color(0.8f, 0.2f, 0.2f); // Berry-like red color
        sr.sortingOrder = 1;

        CircleCollider2D col = foodObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        Food food = foodObj.AddComponent<Food>();
        
        // Randomize type: 70% berry(3), 20% meat(8), 10% crumb(1)
        int rand = Random.Range(0, 100);
        int amount = 3;
        if (rand > 90) amount = 1;
        else if (rand > 70) amount = 8;
        
        food.Init(amount);
        
        activeFoodPiles.Add(food);
    }

    private void CreateCircleSprite()
    {
        // Programmatically generate a simple circle sprite for the food
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float r = size / 2f;
        Vector2 center = new Vector2(r, r);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (Vector2.Distance(center, new Vector2(x, y)) <= r)
                    pixels[y * size + x] = Color.white;
                else
                    pixels[y * size + x] = Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }
}
