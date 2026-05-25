public class Rule_Guard : IJobRule
{
    private Behavior_Guard _behavior;
    public IAntBehavior Behavior => _behavior ??= new Behavior_Guard();
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Soldier };

    public float DemandScore(ColonyBlackboard board)
    {
        return 100f;
    }

    public int MaxAntCap(int totalAnts)
    {
        return int.MaxValue;
    }
}