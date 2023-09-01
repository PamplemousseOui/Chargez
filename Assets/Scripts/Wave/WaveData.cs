using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Serializable]
    public class NumberOfEnemyByType
    {
        public EnemyType type;
        public float minCooldown = 0.0f;
        public float maxCooldown = 5.0f;
    }

    public string friendlyName;
    public float startCooldown = 5.0f;
    public List<NumberOfEnemyByType> numberOfEnemyByTypes;

    
}
