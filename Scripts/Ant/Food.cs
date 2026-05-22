using UnityEngine;

public class Food : MonoBehaviour
{
    public int foodAmount = 5;
    public bool isDepleted = false;
    
    private SpriteRenderer sr;
    private Vector3 initialScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            // Fallback for primitive setup
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        initialScale = transform.localScale;
    }

    public void Init(int amount)
    {
        foodAmount = amount;
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
        // Assuming max food pile is around 10 for scale reference
        float scaleMultiplier = Mathf.Clamp((float)foodAmount / 10f, 0.3f, 1f);
        transform.localScale = initialScale * scaleMultiplier;
    }
}
