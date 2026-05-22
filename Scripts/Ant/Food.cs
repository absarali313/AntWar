using UnityEngine;

public class Food : MonoBehaviour
{
    public int foodAmount = 5;
    public bool isDepleted = false;

    [Header("Visual Scaling")]
    [Tooltip("How small the food visually gets right before it's fully depleted (multiplier)")]
    public float minScaleMultiplier = 0.3f;
    [Tooltip("How large the food is visually when completely full (multiplier)")]
    public float maxScaleMultiplier = 1f;
    
    private SpriteRenderer sr;
    private Vector3 initialScale;
    private int startingFoodAmount;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            // Fallback for primitive setup
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        initialScale = transform.localScale;
        startingFoodAmount = foodAmount;
    }

    public void Init()
    {
        // Reset to initial state
        foodAmount = startingFoodAmount;
        isDepleted = false;
        gameObject.SetActive(true);
        UpdateVisual();
    }

    public int TakeFood(int amount)
    {
        if (isDepleted) return 0;

        int taken = Mathf.Min(amount, foodAmount);
        foodAmount -= taken;

        UpdateVisual();

        if (foodAmount <= 0)
        {
            isDepleted = true;
            gameObject.SetActive(false);
        }

        return taken;
    }

    private void UpdateVisual()
    {
        // Simple scale effect to make food look smaller as it's eaten
        if (startingFoodAmount <= 0) return;
        float ratio = Mathf.Clamp01((float)foodAmount / (float)startingFoodAmount);
        float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, ratio);
        transform.localScale = initialScale * scaleMultiplier;
    }
}
