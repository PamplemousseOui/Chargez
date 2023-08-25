using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public float spawnRate;
    public uint maxEnemyCount;

    private float m_curSpawnTimerValue;
    private bool m_canSpawn;

    public static EventHandler<EnemyType> OnEnemySpawn;
}
