using UnityEngine;

public class Behavior_Guard : IAntBehavior
{
    public string BehaviorName => "Guard";
    public AntRole[] EligibleRoles => new AntRole[] { AntRole.Soldier };

    public bool CanBeInterrupted(Ant ant) => true;

    public void OnAssigned(Ant ant)
    {
        ant.currentPath = null;
    }

    public void Tick(Ant ant)
    {
        SoldierBehavior(ant);
    }

    public bool IsComplete(Ant ant) => false;

    private void SoldierBehavior(Ant ant)
    {
        if (Queen.Instance == null) return;

        Vector2 queenPos = Queen.Instance.transform.position;
        float dist = Vector2.Distance(ant.transform.position, queenPos);

        if (dist > 2f)
        {
            Vector2 dir = (queenPos - (Vector2)ant.transform.position).normalized;
            Vector2 nextPos = (Vector2)ant.transform.position + dir * ant.speed * Time.deltaTime;

            Vector2Int nextPosGrid = GridManager.Instance.WorldToGrid(nextPos);
            Vector2Int currentGrid = GridManager.Instance.WorldToGrid(ant.transform.position);

            if (!GridManager.Instance.IsWalkable(currentGrid))
            {
                ant.transform.position = ant.FindNearestWalkableTo(ant.transform.position);
                return;
            }

            if (GridManager.Instance.IsValid(nextPosGrid.x, nextPosGrid.y) && GridManager.Instance.IsWalkable(nextPosGrid))
            {
                ant.transform.position = nextPos;
                if (dir != Vector2.zero) ant.transform.right = dir;
            }
        }
    }
}