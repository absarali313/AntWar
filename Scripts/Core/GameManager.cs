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
}
