using UnityEngine;

public class Rule_Dig : IJobRule
{
    private Behavior_Dig _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_Dig();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker };

    public float DemandScore(ColonyBlackboard board)
    {
        if (board.pendingDigTargets == 0) return 0f;
        if (board.pendingDigTargets == 1) return 15f;
        return 15f + (board.pendingDigTargets - 1) * 5f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return Mathf.FloorToInt(totalAnts * 0.45f);
    }
}