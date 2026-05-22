using UnityEngine;

public class FoodStorage : MonoBehaviour
{
    public static FoodStorage Instance;

    public int totalFood = 0;
    
    private SpriteRenderer sr;
    private Vector3 initialScale;

    void Awake()
    {
        Instance = this;
        sr = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
    }

    public void Deposit(int amount)
    {
        totalFood += amount;
        UpdateVisual();
        // Here we could trigger Queen consumption or population growth logic in the future
    }

    private void UpdateVisual()
    {
        if (sr != null)
        {
            // Simple visual feedback: storage grows as it gets more food
            float scaleMultiplier = 1f + Mathf.Clamp((float)totalFood / 50f, 0f, 1f); // Max 2x size
            transform.localScale = initialScale * scaleMultiplier;
            
            // Turn slightly more green/yellow as it fills
            sr.color = Color.Lerp(new Color(0.8f, 0.6f, 0.2f), new Color(0.3f, 0.8f, 0.3f), totalFood / 50f);
        }
    }
}
