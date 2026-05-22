using UnityEngine;

public class SwarmManager : MonoBehaviour
{
    public static SwarmManager Instance;

    public Vector2 rallyPoint;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rallyPoint = mousePos;
        }
    }
}
