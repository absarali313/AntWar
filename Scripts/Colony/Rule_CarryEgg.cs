using UnityEngine;

public class Rule_CarryEgg : IJobRule
{
    private Behavior_CarryEgg _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_CarryEgg();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public float DemandScore(ColonyBlackboard board)
    {
        return board.pendingEggs * 20f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return Mathf.Min(1, Mathf.FloorToInt(totalAnts * 0.20f));
    }
}