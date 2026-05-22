using UnityEngine;

public class FoodStorage : MonoBehaviour
{
    public static FoodStorage Instance;

    public int totalFood = 0;
    
    [Header("Visual Settings")]
    public float minScale = 1f;
    public float maxScale = 2f;
    public int foodAmountForMaxScale = 50;
    
    private Vector3 initialScale;

    void Awake()
    {
        Instance = this;
        initialScale = transform.localScale;
    }

    public void Deposit(int amount)
    {
        totalFood += amount;
        UpdateVisual();
        // Here we could trigger Queen consumption or population growth logic in the future
    }

    public bool Consume(int amount)
    {
        if (totalFood >= amount)
        {
            totalFood -= amount;
            UpdateVisual();
            return true;
        }
        return false;
    }

    private void UpdateVisual()
    {
        // Calculate how close we are to the max food limit
        float ratio = 0f;
        if (foodAmountForMaxScale > 0)
        {
            ratio = Mathf.Clamp01((float)totalFood / foodAmountForMaxScale);
        }

        // Scale between minScale and maxScale
        float currentScaleMultiplier = Mathf.Lerp(minScale, maxScale, ratio);
        transform.localScale = initialScale * currentScaleMultiplier;
    }
}
