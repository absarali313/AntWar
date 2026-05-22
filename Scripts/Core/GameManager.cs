using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float survivalTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        survivalTime += Time.deltaTime;
    }

    void OnGUI()
    {
        // Simple built-in UI for prototyping
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // Draw background box for readability
        GUI.Box(new Rect(10, 10, 200, 80), "");

        // Show Survival Time
        int minutes = Mathf.FloorToInt(survivalTime / 60F);
        int seconds = Mathf.FloorToInt(survivalTime - minutes * 60);
        string timeStr = string.Format("{0:00}:{1:00}", minutes, seconds);
        GUI.Label(new Rect(20, 20, 200, 30), "Time: " + timeStr, style);

        // Show Food Count
        int foodCount = 0;
        if (FoodStorage.Instance != null)
        {
            foodCount = FoodStorage.Instance.totalFood;
        }
        
        style.normal.textColor = new Color(0.8f, 0.9f, 0.3f); // Yellow/Green for food
        GUI.Label(new Rect(20, 50, 200, 30), "Food: " + foodCount, style);
    }
}
