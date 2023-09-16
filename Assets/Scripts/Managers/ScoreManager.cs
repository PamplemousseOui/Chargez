using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyKillScore
{
    public EnemyType type;
    public uint score;
}

public class ScoreManager : MonoBehaviour
{
    public uint currentTotalScore;
    public uint fallbackKillScore;
    public List<EnemyKillScore> killScores = new List<EnemyKillScore>();

    private Dictionary<EnemyType, uint> m_enemyKillScoreDict = new Dictionary<EnemyType, uint>();

    private void Start()
    {
        foreach (EnemyKillScore enemyKillScore in killScores)
        {
            if (m_enemyKillScoreDict.ContainsKey(enemyKillScore.type))
            {
                m_enemyKillScoreDict[enemyKillScore.type] = enemyKillScore.score;
            }
            else
                m_enemyKillScoreDict.Add(enemyKillScore.type, enemyKillScore.score);
        }
    }

    private void OnEnable()
    {
        PlayerAttack.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        PlayerAttack.OnEnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled(object sender, EnemyType _enemyType)
    {
        if (m_enemyKillScoreDict.TryGetValue(_enemyType, out uint score))
        {
            currentTotalScore += score;
        }
        else
        {
            currentTotalScore += fallbackKillScore; 
            Debug.LogWarning($"Kill score not found for type {_enemyType}. Using fallback kill score.");
        }
    }
}
