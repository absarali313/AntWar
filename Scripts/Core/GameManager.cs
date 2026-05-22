using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float survivalTime = 0f;

    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI eggCountText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        survivalTime += Time.deltaTime;

        // Update Survival Time UI
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60F);
            int seconds = Mathf.FloorToInt(survivalTime - minutes * 60);
            timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }

        // Update Food Count UI
        if (foodText != null && FoodStorage.Instance != null)
        {
            foodText.text = "Food: " + FoodStorage.Instance.totalFood;
        }

        // Update Egg Count UI
        if (eggCountText != null)
        {
            int waiting = (EggManager.Instance != null) ? EggManager.Instance.availableEggs.Count : 0;
            int incubating = (Nursery.Instance != null) ? Nursery.Instance.eggsIncubating : 0;
            eggCountText.text = "Eggs: " + waiting + " | Hatching: " + incubating;
        }
    }
}
