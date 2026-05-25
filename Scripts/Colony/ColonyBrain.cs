using UnityEngine;
using System.Collections.Generic;

public class ColonyBrain : MonoBehaviour
{
    public static ColonyBrain Instance { get; private set; }

    [Header("Settings")]
    public float rebalanceInterval = 3f;
    
    private ColonyBlackboard blackboard;
    private List<IJobRule> rules = new List<IJobRule>();
    private List<Ant> allAnts = new List<Ant>();
    private Behavior_Idle idleBehavior;
    private float rebalanceTimer = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        blackboard = ColonyBlackboard.Instance;
        idleBehavior = new Behavior_Idle();

        RegisterRule(new Rule_Dig());
        RegisterRule(new Rule_CarryEgg());
        RegisterRule(new Rule_Food());
        RegisterRule(new Rule_Water());
        RegisterRule(new Rule_Minerals());
        RegisterRule(new Rule_Guard());
    }

    void Update()
    {
        rebalanceTimer -= Time.deltaTime;
        if (rebalanceTimer <= 0f)
        {
            Rebalance();
            rebalanceTimer = rebalanceInterval;
        }
    }

    public void RegisterAnt(Ant ant)
    {
        if (!allAnts.Contains(ant))
        {
            allAnts.Add(ant);
        }
    }

    public void UnregisterAnt(Ant ant)
    {
        allAnts.Remove(ant);
    }

    private void RegisterRule(IJobRule rule)
    {
        rules.Add(rule);
    }

    private void Rebalance()
    {
        blackboard.Refresh(allAnts);

        List<(IJobRule rule, float score)> scoredRules = new List<(IJobRule, float)>();
        foreach (var rule in rules)
        {
            float score = rule.DemandScore(blackboard);
            if (score > 0)
            {
                scoredRules.Add((rule, score));
            }
        }
        scoredRules.Sort((a, b) => b.score.CompareTo(a.score));

        List<Ant> pool = new List<Ant>();
        foreach (Ant ant in allAnts)
        {
            if (ant.currentBehavior == null)
            {
                pool.Add(ant);
            }
            else if (!ant.currentBehavior.IsComplete(ant))
            {
                // Job not complete - can potentially interrupt
                if (ant.currentBehavior.CanBeInterrupted(ant))
                {
                    pool.Add(ant);
                }
            }
            else
            {
                // Job is complete - must be reassigned
                pool.Add(ant);
            }
        }

        foreach (var (rule, score) in scoredRules)
        {
            int alreadyAssigned = 0;
            string behaviorName = rule.Behavior.BehaviorName;
            if (blackboard.antsByBehavior.ContainsKey(behaviorName))
            {
                alreadyAssigned = blackboard.antsByBehavior[behaviorName];
            }

            int cap = rule.MaxAntCap(blackboard.totalAnts);
            int needed = Mathf.Max(0, cap - alreadyAssigned);
            int toAssign = Mathf.Min(needed, pool.Count);

            for (int i = 0; i < toAssign; i++)
            {
                Ant ant = FindBestAntFromPool(pool, rule);
                if (ant != null)
                {
                    AssignBehavior(ant, rule.Behavior);
                    pool.Remove(ant);
                }
            }
        }

        foreach (Ant ant in pool)
        {
            AssignBehavior(ant, idleBehavior);
        }
    }

    private Ant FindBestAntFromPool(List<Ant> pool, IJobRule rule)
    {
        foreach (Ant ant in pool)
        {
            foreach (AntRole role in rule.EligibleRoles)
            {
                if (ant.role == role)
                {
                    return ant;
                }
            }
        }
        return null;
    }

    private void AssignBehavior(Ant ant, IAntBehavior behavior)
    {
        if (ant.currentBehavior?.BehaviorName == behavior.BehaviorName) return;
        ant.SetBehavior(behavior);
    }
}