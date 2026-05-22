using UnityEngine;
using System.Collections.Generic;

public class EggManager : MonoBehaviour
{
    public static EggManager Instance;

    public List<Egg> availableEggs = new List<Egg>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Cleanup destroyed eggs
        availableEggs.RemoveAll(e => e == null);
    }

    public void RegisterEgg(Egg egg)
    {
        if (!availableEggs.Contains(egg))
        {
            availableEggs.Add(egg);
        }
    }

    public void UnregisterEgg(Egg egg)
    {
        availableEggs.Remove(egg);
    }
}
