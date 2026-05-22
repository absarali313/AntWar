using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QueenUI : MonoBehaviour
{
    public static QueenUI Instance;

    [Header("Panel")]
    public GameObject queenPanel;

    [Header("Buttons")]
    public Button spawnWorkerButton;
    public Button closeButton;

    [Header("Progress")]
    public GameObject progressBarContainer;
    public Image progressBarFill;
    public TextMeshProUGUI statusText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Hide panel initially
        if (queenPanel != null)
            queenPanel.SetActive(false);

        // Wire up buttons
        if (spawnWorkerButton != null)
            spawnWorkerButton.onClick.AddListener(OnSelectWorker);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);
    }

    void Update()
    {
        // Update progress bar if Queen is laying
        if (Queen.Instance != null && progressBarContainer != null)
        {
            if (Queen.Instance.isLayingEgg)
            {
                progressBarContainer.SetActive(true);
                if (progressBarFill != null)
                    progressBarFill.fillAmount = Queen.Instance.layingProgress;
                if (statusText != null)
                    statusText.text = "Laying egg... " + Mathf.FloorToInt(Queen.Instance.layingProgress * 100f) + "%";
            }
            else
            {
                progressBarContainer.SetActive(false);
                if (statusText != null)
                {
                    EggProductionConfig workerConfig = Queen.Instance.GetConfig(AntType.Worker);
                    int cost = workerConfig != null ? workerConfig.foodCost : 0;
                    statusText.text = "Ready (Cost: " + cost + " Food)";
                }
            }

            // Update spawn button interactability
            if (spawnWorkerButton != null)
            {
                spawnWorkerButton.interactable = !Queen.Instance.isLayingEgg && Queen.Instance.CanAffordEgg(AntType.Worker);
            }
        }
    }

    public void OpenMenu()
    {
        if (queenPanel != null)
            queenPanel.SetActive(true);
    }

    public void CloseMenu()
    {
        if (queenPanel != null)
            queenPanel.SetActive(false);
    }

    void OnSelectWorker()
    {
        if (Queen.Instance != null)
        {
            Queen.Instance.StartLayingEgg(AntType.Worker);
        }
    }
}
