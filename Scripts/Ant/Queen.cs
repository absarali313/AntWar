using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EggProductionConfig
{
    public AntType type;
    [Tooltip("Time it takes to lay this egg")]
    public float layingTime = 5f;
    [Tooltip("Food cost to lay this egg")]
    public int foodCost = 15;
    [Tooltip("Optional: Specific prefab for this egg type (overrides default)")]
    public GameObject specificEggPrefab;
}

public class Queen : MonoBehaviour
{
    public static Queen Instance;

    public int health = 100;

    [Header("Egg Production Settings")]
    public GameObject defaultEggPrefab;
    public List<EggProductionConfig> productionConfigs = new List<EggProductionConfig>()
    {
        new EggProductionConfig { type = AntType.Worker, layingTime = 5f, foodCost = 15 }
    };

    [Header("Status (Read Only)")]
    public bool isLayingEgg = false;
    public float layingProgress = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (health <= 0)
        {
            Debug.Log("COLONY DESTROYED");
        }
    }

    void OnMouseDown()
    {
        // Open the Queen production menu
        if (QueenUI.Instance != null)
        {
            QueenUI.Instance.OpenMenu();
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Queen HP: " + health);
    }

    public EggProductionConfig GetConfig(AntType type)
    {
        return productionConfigs.Find(c => c.type == type);
    }

    public bool CanAffordEgg(AntType type)
    {
        EggProductionConfig config = GetConfig(type);
        if (config == null) return false;
        
        if (FoodStorage.Instance == null) return false;
        return FoodStorage.Instance.totalFood >= config.foodCost;
    }

    public void StartLayingEgg(AntType type)
    {
        if (isLayingEgg) return;

        EggProductionConfig config = GetConfig(type);
        if (config == null)
        {
            Debug.LogWarning("Queen: No production config found for " + type);
            return;
        }

        if (FoodStorage.Instance != null && FoodStorage.Instance.Consume(config.foodCost))
        {
            StartCoroutine(LayEggRoutine(type, config.layingTime));
        }
        else
        {
            Debug.Log("Not enough food to lay egg!");
        }
    }

    IEnumerator LayEggRoutine(AntType type, float layingTime)
    {
        isLayingEgg = true;
        layingProgress = 0f;

        float elapsed = 0f;
        while (elapsed < layingTime)
        {
            elapsed += Time.deltaTime;
            layingProgress = Mathf.Clamp01(elapsed / layingTime);
            yield return null;
        }

        SpawnEgg(type);

        isLayingEgg = false;
        layingProgress = 0f;
    }

    void SpawnEgg(AntType type)
    {
        EggProductionConfig config = GetConfig(type);
        GameObject prefabToUse = (config != null && config.specificEggPrefab != null) ? config.specificEggPrefab : defaultEggPrefab;

        if (prefabToUse == null)
        {
            Debug.LogWarning("Queen: No egg prefab assigned for type " + type);
            return;
        }

        // Spawn egg slightly offset from Queen
        Vector2 offset = Random.insideUnitCircle * 0.5f;
        GameObject eggObj = Instantiate(prefabToUse, (Vector2)transform.position + offset, Quaternion.identity);

        Egg egg = eggObj.GetComponent<Egg>();
        if (egg == null) egg = eggObj.AddComponent<Egg>();

        egg.type = type;

        // Register with EggManager
        if (EggManager.Instance != null)
        {
            EggManager.Instance.RegisterEgg(egg);
        }
    }
}
