using UnityEngine;
using System.Collections.Generic;

public class HarvestCommandManager : MonoBehaviour
{
    public static HarvestCommandManager Instance;

    private Ant selectedAnt = null;
    private readonly List<AssignedAnt> assignments = new List<AssignedAnt>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    void HandleLeftClick()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero);
        Debug.Log($"Raycast hits: {hits.Length}");

        if (hits.Length == 0)
        {
            Deselect();
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Debug.Log($"Hit object: {hits[i].collider.name}, layer: {LayerMask.LayerToName(hits[i].collider.gameObject.layer)}");
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Ant ant = hits[i].collider.GetComponent<Ant>();
            if (ant != null)
            {
                Debug.Log($"Selected ant");
                SelectAnt(ant);
                return;
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Food food = hits[i].collider.GetComponent<Food>() ?? hits[i].collider.GetComponentInParent<Food>() ?? hits[i].collider.GetComponentInChildren<Food>();
            if (food != null)
            {
                Debug.Log($"Found food on: {food.name}");
                if (selectedAnt != null)
                {
                    AssignAntToFood(selectedAnt, food);
                }
                return;
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            WaterSource water = hits[i].collider.GetComponent<WaterSource>() ?? hits[i].collider.GetComponentInParent<WaterSource>() ?? hits[i].collider.GetComponentInChildren<WaterSource>();
            if (water != null)
            {
                Debug.Log($"Found water on: {water.name}");
                if (selectedAnt != null)
                {
                    AssignAntToWater(selectedAnt, water);
                }
                return;
            }
        }

        for (int i = 0; i < hits.Length; i++)
        {
            MineralDeposit mineral = hits[i].collider.GetComponent<MineralDeposit>() ?? hits[i].collider.GetComponentInParent<MineralDeposit>() ?? hits[i].collider.GetComponentInChildren<MineralDeposit>();
            if (mineral != null)
            {
                Debug.Log($"Found mineral on: {mineral.name}");
                if (selectedAnt != null)
                {
                    AssignAntToMineral(selectedAnt, mineral);
                }
                return;
            }
        }

        Deselect();
    }

    void HandleRightClick()
    {
        if (selectedAnt != null)
        {
            UnpinSelectedAnt();
        }
        else
        {
            Deselect();
        }
    }

    void SelectAnt(Ant ant)
    {
        selectedAnt = ant;
        ShowSelectionHighlight(ant);
    }

    void Deselect()
    {
        selectedAnt = null;
        ClearSelectionHighlight();
    }

    public static void ReleaseAssignment(Ant ant)
    {
        if (Instance != null)
        {
            Instance.ReleaseAnt(ant);
        }
        else
        {
            ant.isPinned = false;
            ant.SetBehavior(new Behavior_Idle());
        }
    }

    void ReleaseAnt(Ant ant)
    {
        for (int i = assignments.Count - 1; i >= 0; i--)
        {
            if (assignments[i].ant == ant)
            {
                assignments.RemoveAt(i);
                ant.isPinned = false;
                ant.SetBehavior(new Behavior_Idle());
                ClearAssignedVisual(ant);
                return;
            }
        }
    }

    bool IsAntAssigned(Ant ant)
    {
        foreach (var a in assignments)
        {
            if (a.ant == ant) return true;
        }
        return false;
    }

    void AssignAntToFood(Ant ant, Food food)
    {
        Debug.Log($"AssignAntToFood called - ant role: {ant.role}, is Worker: {ant.role == AntRole.Worker}");
        if (ant.role != AntRole.Worker)
        {
            Debug.Log("Not a worker, deselecting");
            Deselect();
            return;
        }

        ClearOldAssignment(ant);

        ant.targetFood = food;
        ant.targetWater = null;
        ant.targetMineral = null;
        ant.isPinned = true;
        ant.SetBehavior(new Behavior_CollectFood());

        assignments.Add(new AssignedAnt { ant = ant, targetType = ResourceType.Food, targetObject = food.gameObject });
        ShowAssignedVisual(ant, food.gameObject);

        Deselect();
        Debug.Log("Assigned ant to food");
    }

    void AssignAntToWater(Ant ant, WaterSource water)
    {
        if (ant.role != AntRole.Worker)
        {
            Deselect();
            return;
        }

        ClearOldAssignment(ant);

        ant.targetWater = water;
        ant.targetFood = null;
        ant.targetMineral = null;
        ant.isPinned = true;
        ant.SetBehavior(new Behavior_CollectWater());

        assignments.Add(new AssignedAnt { ant = ant, targetType = ResourceType.Water, targetObject = water.gameObject });
        ShowAssignedVisual(ant, water.gameObject);

        Deselect();
    }

    void AssignAntToMineral(Ant ant, MineralDeposit mineral)
    {
        if (ant.role != AntRole.Worker)
        {
            Deselect();
            return;
        }

        ClearOldAssignment(ant);

        ant.targetMineral = mineral;
        ant.targetFood = null;
        ant.targetWater = null;
        ant.isPinned = true;
        ant.SetBehavior(new Behavior_CollectMinerals());

        assignments.Add(new AssignedAnt { ant = ant, targetType = ResourceType.Minerals, targetObject = mineral.gameObject });
        ShowAssignedVisual(ant, mineral.gameObject);

        Deselect();
    }

    void ClearOldAssignment(Ant ant)
    {
        for (int i = assignments.Count - 1; i >= 0; i--)
        {
            if (assignments[i].ant == ant)
            {
                assignments.RemoveAt(i);
                ant.isPinned = false;
                ant.currentPath = null;
                ClearAssignedVisual(ant);
                break;
            }
        }
    }

    void UnpinSelectedAnt()
    {
        Ant ant = selectedAnt;
        Deselect();
        ReleaseAnt(ant);
    }

    void ShowSelectionHighlight(Ant ant)
    {
        var sr = ant.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow;
        }
    }

    void ClearSelectionHighlight()
    {
        if (selectedAnt != null)
        {
            var sr = selectedAnt.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }

    void ShowAssignedVisual(Ant ant, GameObject target)
    {
        var sr = ant.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.cyan;
        }
    }

    void ClearAssignedVisual(Ant ant)
    {
        var sr = ant.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }

    void OnDestroy()
    {
        assignments.Clear();
    }

    private class AssignedAnt
    {
        public Ant ant;
        public ResourceType targetType;
        public GameObject targetObject;
    }
}