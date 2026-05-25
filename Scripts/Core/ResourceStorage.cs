using UnityEngine;
using System.Collections.Generic;

public class ResourceStorage : MonoBehaviour
{
    public static ResourceStorage Instance;

    [System.Serializable]
    public class ResourceEntry
    {
        public ResourceType type;
        public int amount;
    }

    public Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>()
    {
        { ResourceType.Food, 100 },
        { ResourceType.Water, 100 },
        { ResourceType.Minerals, 100 }
    };

    [Header("Visual Settings")]
    public float minScale = 1f;
    public float maxScale = 2f;
    public int maxAmountForMaxScale = 50;

    private Vector3 initialScale;

    void Awake()
    {
        Instance = this;
        initialScale = transform.localScale;
    }

    public int Get(ResourceType type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    public void Deposit(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;
        
        resources[type] += amount;
        UpdateVisual();
    }

    public bool Consume(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type) && resources[type] >= amount)
        {
            resources[type] -= amount;
            UpdateVisual();
            return true;
        }
        return false;
    }

    private void UpdateVisual()
    {
        int total = 0;
        foreach (var kvp in resources)
            total += kvp.Value;

        float ratio = 0f;
        if (maxAmountForMaxScale > 0)
        {
            ratio = Mathf.Clamp01((float)total / maxAmountForMaxScale);
        }

        float currentScaleMultiplier = Mathf.Lerp(minScale, maxScale, ratio);
        transform.localScale = initialScale * currentScaleMultiplier;
    }
}
