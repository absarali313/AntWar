using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float survivalTime = 0f;

    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI mineralText;
    public TextMeshProUGUI eggCountText;
    public TextMeshProUGUI antJobText;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        survivalTime += Time.deltaTime;

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60F);
            int seconds = Mathf.FloorToInt(survivalTime - minutes * 60);
            timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }

        if (foodText != null && ResourceStorage.Instance != null)
        {
            foodText.text = "Food: " + ResourceStorage.Instance.Get(ResourceType.Food);
        }

        if (waterText != null && ResourceStorage.Instance != null)
        {
            waterText.text = "Water: " + ResourceStorage.Instance.Get(ResourceType.Water);
        }

        if (mineralText != null && ResourceStorage.Instance != null)
        {
            mineralText.text = "Minerals: " + ResourceStorage.Instance.Get(ResourceType.Minerals);
        }

        if (eggCountText != null)
        {
            int waiting = (EggManager.Instance != null) ? EggManager.Instance.availableEggs.Count : 0;
            int incubating = (Nursery.Instance != null) ? Nursery.Instance.eggsIncubating : 0;
            eggCountText.text = "Eggs: " + waiting + " | Hatching: " + incubating;
        }

        if (antJobText != null && ColonyBlackboard.Instance != null)
        {
            var bb = ColonyBlackboard.Instance;
            string display = "";
            foreach (var kvp in bb.antsByBehavior)
            {
                display += kvp.Key + ": " + kvp.Value + "  |  ";
            }
            antJobText.text = display.Trim(' ', '|');
        }
    }
}