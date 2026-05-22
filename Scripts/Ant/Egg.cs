using UnityEngine;

public enum AntType
{
    Worker
}

public class Egg : MonoBehaviour
{
    public AntType type = AntType.Worker;
    public bool isPickedUp = false;

    public void PickUp()
    {
        isPickedUp = true;
    }

    public void Deliver()
    {
        // Egg is consumed when delivered to nursery
        if (EggManager.Instance != null)
        {
            EggManager.Instance.UnregisterEgg(this);
        }
        Destroy(gameObject);
    }
}
