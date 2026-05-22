using UnityEngine;

public class Queen : MonoBehaviour
{
    public static Queen Instance;

    public int health = 100;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (health <= 0)
        {
            Debug.Log("COLONY DESTROYED");
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Queen HP: " + health);
    }
}
