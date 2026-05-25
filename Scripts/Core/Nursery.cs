using UnityEngine;
using System.Collections;

public class Nursery : MonoBehaviour
{
    public static Nursery Instance;

    [Header("Hatching Settings")]
    public float hatchTime = 5f;

    [Header("Prefabs")]
    public GameObject antPrefab;

    [Header("Status")]
    public int eggsIncubating = 0;

    void Awake()
    {
        Instance = this;
    }

    public void ReceiveEgg(Egg egg)
    {
        AntType type = egg.type;
        egg.Deliver(); // Destroys the egg object
        StartCoroutine(HatchEgg(type));
    }

    IEnumerator HatchEgg(AntType type)
    {
        eggsIncubating++;

        yield return new WaitForSeconds(hatchTime);

        SpawnAnt(type);
        eggsIncubating--;
    }

    void SpawnAnt(AntType type)
    {
        if (antPrefab == null)
        {
            Debug.LogWarning("Nursery: antPrefab is not assigned!");
            return;
        }

        Vector2 offset = Random.insideUnitCircle * 0.5f;
        GameObject antObj = Instantiate(antPrefab, (Vector2)transform.position + offset, Quaternion.identity);

        Ant ant = antObj.GetComponent<Ant>();
        if (ant != null)
        {
            switch (type)
            {
                case AntType.Worker:
                    ant.role = AntRole.Worker;
                    break;
                case AntType.Soldier:
                    ant.role = AntRole.Soldier;
                    break;
                case AntType.Scout:
                    ant.role = AntRole.Scout;
                    break;
                case AntType.Builder:
                    ant.role = AntRole.Builder;
                    break;
                case AntType.Nurse:
                    ant.role = AntRole.Nurse;
                    break;
            }
        }
    }
}
