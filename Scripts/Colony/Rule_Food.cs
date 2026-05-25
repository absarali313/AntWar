using UnityEngine;

// Reserved for AI assignment - player harvesting uses HarvestCommandManager
public class Rule_Food : IJobRule
{
    private Behavior_CollectFood _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_CollectFood();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public float DemandScore(ColonyBlackboard board)
    {
        if (board.foodSourcesInWorld == 0) return 0f;
        if (board.foodStored > 30) return 3f;
        if (board.foodStored >= 15) return 12f;
        if (board.foodStored >= 5) return 25f;
        return 50f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return totalAnts;
    }
}