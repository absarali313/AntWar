using UnityEngine;

public class MineralDeposit : MonoBehaviour
{
    public int mineralAmount = 10;
    public bool isDepleted = false;

    [Header("Visual Scaling")]
    public float minScaleMultiplier = 0.3f;
    public float maxScaleMultiplier = 1f;

    private SpriteRenderer sr;
    private Vector3 initialScale;
    private int startingMineralAmount;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        initialScale = transform.localScale;
        startingMineralAmount = mineralAmount;
    }

    public void Init()
    {
        mineralAmount = startingMineralAmount;
        isDepleted = false;
        gameObject.SetActive(true);
        UpdateVisual();
    }

    public int TakeMinerals(int amount)
    {
        if (isDepleted) return 0;

        int taken = Mathf.Min(amount, mineralAmount);
        mineralAmount -= taken;

        UpdateVisual();

        if (mineralAmount <= 0)
        {
            isDepleted = true;
            gameObject.SetActive(false);
        }

        return taken;
    }

    private void UpdateVisual()
    {
        if (startingMineralAmount <= 0) return;
        float ratio = Mathf.Clamp01((float)mineralAmount / (float)startingMineralAmount);
        float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, ratio);
        transform.localScale = initialScale * scaleMultiplier;
    }
}