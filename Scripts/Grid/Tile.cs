using UnityEngine;

public enum FogState
{
    Unseen,
    Explored,
    Visible
}

public class Tile : MonoBehaviour
{
    public FogState fogState = FogState.Unseen;
    public int x;
    public int y;
    public Vector2Int gridPosition;

    public bool blocked = true;
    public float pheromone = 0f;

    public int maxHealth = 3;
    public int health = 3;
    public bool isDug => !blocked;

    public void Damage(int amt)
    {
        if (!blocked) return;
        health -= amt;
        if (health <= 0)
        {
            Dig();
        }
        else
        {
            // Show dig progress — tile gets lighter as health drops
            UpdateVisual();
        }
    }

    public void Repair()
    {
        blocked = true;
        health = 3;
        UpdateVisual();
    }

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y)
    {
        this.x = x;
        this.y = y;
        gridPosition = new Vector2Int(x, y);
        blocked = true;
        fogState = FogState.Unseen;

        UpdateVisual();
    }

    public void Dig()
    {
        if (blocked)
        {
            blocked = false;
            if (GridManager.Instance != null)
            {
                GridManager.Instance.AddWalkableTile(gridPosition);
            }
        }
        UpdateVisual();
    }

    // No per-frame updates needed — visuals update on state change only

    public void UpdateVisual()
    {
        Color baseColor;

        if (blocked)
        {
            // Show dig progress: soil gets lighter/cracked as health drops
            float damageRatio = 1f - ((float)health / maxHealth); // 0 = full health, 1 = about to break
            Color soil = new Color(0.15f, 0.15f, 0.15f);
            Color cracked = new Color(0.4f, 0.3f, 0.2f); // brownish "cracked" look
            baseColor = Color.Lerp(soil, cracked, damageRatio);
        }
        else
        {
            // pheromone visualization only (no interaction logic)
            float p = Mathf.Clamp01(pheromone);

            baseColor = Color.Lerp(
                new Color(0.6f, 0.6f, 0.6f),
                Color.green,
                p
            );
        }

        // Apply Fog tint over the base color
        switch (fogState)
        {
            case FogState.Unseen:
                sr.color = Color.black * 0.2f;
                break;
            case FogState.Explored:
                if (!blocked) 
                    sr.color = baseColor; // No fog over dug paths!
                else 
                    sr.color = baseColor * 0.5f; // Dimmed memory for solid dirt
                break;
            case FogState.Visible:
                sr.color = baseColor; // Fully lit
                break;
        }
    }

    public void UpdateFogVisual()
    {
        UpdateVisual();
    }

    public void SetExplored()
    {
        if (fogState == FogState.Unseen)
            fogState = FogState.Explored;

        UpdateFogVisual();
    }

    public void Reveal()
    {
        fogState = FogState.Visible;
        if (ExplorationMap.Instance != null)
            ExplorationMap.Instance.MarkExplored(GridPos());
    }

    public Vector2Int GridPos()
    {
        return gridPosition;
    }
}
