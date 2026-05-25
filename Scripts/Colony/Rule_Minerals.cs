using UnityEngine;

// Reserved for AI assignment - player harvesting uses HarvestCommandManager
public class Rule_Minerals : IJobRule
{
    private Behavior_CollectMinerals _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_CollectMinerals();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public float DemandScore(ColonyBlackboard board)
    {
        if (board.mineralSourcesInWorld == 0) return 0f;
        if (board.mineralsStored > 30) return 1f;
        if (board.mineralsStored >= 15) return 5f;
        if (board.mineralsStored >= 5) return 12f;
        return 15f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return totalAnts;
    }
}