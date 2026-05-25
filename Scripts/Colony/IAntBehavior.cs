public interface IAntBehavior
{
    string BehaviorName { get; }
    AntRole[] EligibleRoles { get; }
    bool CanBeInterrupted(Ant ant);
    void OnAssigned(Ant ant);
    void Tick(Ant ant);
    bool IsComplete(Ant ant);
}