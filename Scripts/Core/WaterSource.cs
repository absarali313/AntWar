using UnityEngine;

public class WaterSource : MonoBehaviour
{
    public int waterAmount = 8;
    public bool isDepleted = false;

    [Header("Visual Scaling")]
    public float minScaleMultiplier = 0.3f;
    public float maxScaleMultiplier = 1f;

    private SpriteRenderer sr;
    private Vector3 initialScale;
    private int startingWaterAmount;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        initialScale = transform.localScale;
        startingWaterAmount = waterAmount;
    }

    public void Init()
    {
        waterAmount = startingWaterAmount;
        isDepleted = false;
        gameObject.SetActive(true);
        UpdateVisual();
    }

    public int TakeWater(int amount)
    {
        if (isDepleted) return 0;

        int taken = Mathf.Min(amount, waterAmount);
        waterAmount -= taken;

        UpdateVisual();

        if (waterAmount <= 0)
        {
            isDepleted = true;
            gameObject.SetActive(false);
        }

        return taken;
    }

    private void UpdateVisual()
    {
        if (startingWaterAmount <= 0) return;
        float ratio = Mathf.Clamp01((float)waterAmount / (float)startingWaterAmount);
        float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, ratio);
        transform.localScale = initialScale * scaleMultiplier;
    }
}