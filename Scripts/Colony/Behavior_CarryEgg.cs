using System.Collections.Generic;
using UnityEngine;

public class Behavior_CarryEgg : IAntBehavior
{
    public string BehaviorName => "CarryEgg";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public bool CanBeInterrupted(Ant ant)
    {
        return ant.carriedEgg != null;
    }

    public void OnAssigned(Ant ant)
    {
        ant.targetEgg = null;
    }

    public void Tick(Ant ant)
    {
        if (ant.carriedEgg != null)
        {
            DeliverEggToNursery(ant);
            return;
        }

        if (ant.targetEgg == null || ant.targetEgg.isPickedUp)
        {
            FindEgg(ant);
        }

        if (ant.targetEgg != null && !ant.targetEgg.isPickedUp)
        {
            float dist = Vector2.Distance(ant.transform.position, ant.targetEgg.transform.position);
            if (dist < 0.5f)
            {
                PickUpEgg(ant);
            }
            else
            {
                ant.FollowPathTo(ant.targetEgg.transform.position);
            }
        }
    }

    public bool IsComplete(Ant ant)
    {
        return ant.carriedEgg == null && ant.targetEgg == null && EggManager.Instance == null;
    }

    private void FindEgg(Ant ant)
    {
        ant.targetEgg = null;
        if (EggManager.Instance == null) return;

        float bestDist = float.MaxValue;
        float searchRadius = 20f;

        foreach (Egg e in EggManager.Instance.availableEggs)
        {
            if (e != null && !e.isPickedUp)
            {
                float d = Vector2.Distance(ant.transform.position, e.transform.position);
                if (d <= searchRadius && d < bestDist)
                {
                    bestDist = d;
                    ant.targetEgg = e;
                }
            }
        }
    }

    private void PickUpEgg(Ant ant)
    {
        if (ant.targetEgg == null || ant.targetEgg.isPickedUp) return;

        ant.targetEgg.PickUp();
        ant.carriedEgg = ant.targetEgg;
        ant.targetEgg = null;
        ant.currentPath = null;

        ant.carriedEgg.transform.SetParent(ant.transform);
        ant.carriedEgg.transform.localPosition = new Vector3(0, 0.3f, 0);
    }

    private void DeliverEggToNursery(Ant ant)
    {
        if (Nursery.Instance == null) return;

        Vector2 nurseryPos = Nursery.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, nurseryPos);

        if (dist < 1.0f)
        {
            Nursery.Instance.ReceiveEgg(ant.carriedEgg);
            ant.carriedEgg = null;
            ant.currentPath = null;
        }
        else
        {
            ant.FollowPathTo(nurseryPos);
        }
    }
}