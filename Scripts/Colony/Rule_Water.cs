using UnityEngine;

// Reserved for AI assignment - player harvesting uses HarvestCommandManager
public class Rule_Water : IJobRule
{
    private Behavior_CollectWater _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_CollectWater();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public float DemandScore(ColonyBlackboard board)
    {
        if (board.waterSourcesInWorld == 0) return 0f;
        if (board.waterStored > 30) return 2f;
        if (board.waterStored >= 15) return 8f;
        if (board.waterStored >= 5) return 20f;
        return 30f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return totalAnts;
    }
}