public interface IJobRule
{
    IAntBehavior Behavior { get; }
    AntRole[] EligibleRoles { get; }
    float DemandScore(ColonyBlackboard board);
    int MaxAntCap(int totalAnts);
}