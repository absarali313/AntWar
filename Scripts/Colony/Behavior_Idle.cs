using UnityEngine;

public class Behavior_Idle : IAntBehavior
{
    public string BehaviorName => "Idle";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Worker, AntRole.Soldier, AntRole.Scout, AntRole.Builder, AntRole.Nurse };

    public bool CanBeInterrupted(Ant ant) => true;

    public void OnAssigned(Ant ant)
    {
        ant.PickNewWanderTarget();
    }

    public void Tick(Ant ant)
    {
        IdleBehavior(ant);
    }

    public bool IsComplete(Ant ant) => false;

    private void IdleBehavior(Ant ant)
    {
        ant.wanderTimer -= Time.deltaTime;

        if (ant.wanderTimer <= 0f)
        {
            ant.PickNewWanderTarget();
        }

        float distToTarget = Vector2.Distance(ant.transform.position, ant.wanderTarget);
        if (distToTarget < 0.3f)
        {
            ant.PickNewWanderTarget();
            return;
        }

        Vector2 dir = (ant.wanderTarget - (Vector2)ant.transform.position).normalized;
        Vector2 nextPos = (Vector2)ant.transform.position + dir * ant.speed * Time.deltaTime;

        Vector2Int nextPosGrid = GridManager.Instance.WorldToGrid(nextPos);
        Vector2Int currentGrid = GridManager.Instance.WorldToGrid(ant.transform.position);

        if (!GridManager.Instance.IsWalkable(currentGrid))
        {
            ant.transform.position = ant.FindNearestWalkableTo(ant.transform.position);
            ant.PickNewWanderTarget();
            return;
        }

        if (GridManager.Instance.IsValid(nextPosGrid.x, nextPosGrid.y) && GridManager.Instance.IsWalkable(nextPosGrid))
        {
            ant.transform.position = nextPos;
            if (dir != Vector2.zero) ant.transform.right = dir;
        }
        else
        {
            ant.PickNewWanderTarget();
        }
    }
}